using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Fwk.Addressables
{
    public class AddressableCache : IDisposable
    {
        private readonly Dictionary<string, object> _handles = new();
        private readonly Dictionary<string, UniTask> _loadingTasks = new();
        private CancellationTokenSource _disposeCts = new();
        private bool _isDisposed = false;

        public async UniTask<T> LoadAsync<T>(
            string key,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        ) where T : UnityEngine.Object
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AddressableCache));
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);

            while (true)
            {
                if (TryGetHandle<T>(key, out AddressableHandle<T> handle))
                {
                    if (handle.Asset != null)
                    {
                        return handle.Asset;
                    }
                    else
                    {
                        Debug.LogWarning($"Asset of key '{key}' is null. Releasing the handle.");
                        handle.Release();
                        _handles.Remove(key);
                    }
                }

                if (_loadingTasks.TryGetValue(key, out var loadingTask))
                {
                    try
                    {
                        await loadingTask;
                    }
                    catch
                    {
                        // continue if existing loading task failed
                    }
                    continue;
                }

                try
                {
                    var task = LoadAsyncInternal();
                    _loadingTasks[key] = task;
                    await task;
                }
                catch (OperationCanceledException)
                {
                    if (TryGetHandle<T>(key, out var existingHandle))
                    {
                        Debug.LogWarning($"Key '{key}' was canceled. Releasing the existing handle.");
                        existingHandle.Release();
                        _handles.Remove(key);
                    }
                    throw;
                }
                finally
                {
                    _loadingTasks.Remove(key);
                }

                if (TryGetHandle<T>(key, out var loadedHandle))
                {
                    return loadedHandle.Asset;
                }
                else
                {
                    Debug.LogError($"Failed to load asset of key '{key}' after loading task completed.");
                    throw new Exception($"Failed to load asset of key '{key}'.");
                }
            }

            async UniTask LoadAsyncInternal()
            {
                try
                {
                    var newHandle = await AddressableManager.LoadAsync<T>(key, progress, linkedCts.Token);

                    if (newHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"Failed to load asset of key '{key}'.");
                        throw new Exception($"Failed to load asset of key '{key}'.");
                    }

                    if (TryGetHandle<T>(key, out var existingHandle))
                    {
                        Debug.LogWarning($"Key '{key}' already exists in the cache. Releasing the new handle.");
                        newHandle.Release();
                    }
                    else
                    {
                        _handles[key] = newHandle;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (TryGetHandle<T>(key, out var existingHandle))
                    {
                        existingHandle.Release();
                        _handles.Remove(key);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (TryGetHandle<T>(key, out var existingHandle))
                    {
                        existingHandle.Release();
                        _handles.Remove(key);
                    }
                    Debug.LogError($"Failed to load asset of key '{key}': {ex}");
                    throw;
                }
            }
        }

        private bool TryGetHandle<T>(string key, out AddressableHandle<T> handle) where T : UnityEngine.Object
        {
            if (_handles.TryGetValue(key, out var boxedHandle))
            {
                if (boxedHandle is AddressableHandle<T> existingHandle)
                {
                    handle = existingHandle;
                    return true;
                }
                else
                {
                    Debug.LogError($"Handle of Key '{key}' is of a different type. Cannot cast to {typeof(T)}.");
                }
            }
            handle = null;
            return false;
        }

        public void Release(string key)
        {
            if (_handles.TryGetValue(key, out var boxedHandle))
            {
                if (boxedHandle is AddressableHandle<UnityEngine.Object> handle)
                {
                    handle.Release();
                }
                _handles.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var boxedHandle in _handles.Values)
            {
                if (boxedHandle is AddressableHandle<UnityEngine.Object> handle)
                {
                    handle.Release();
                }
            }
            _handles.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            _disposeCts?.Cancel();
            _disposeCts?.Dispose();
            _disposeCts = null;

            ReleaseAll();
            _loadingTasks.Clear();
        }
    }
}
