using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Fwk
{
    public class AddressableCache
    {
        private readonly Dictionary<string, object> _handles = new();
        private readonly Dictionary<string, UniTask> _loadingTasks = new();

        public async UniTask<T> LoadAsync<T>(
            string key,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        ) where T : UnityEngine.Object
        {
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
                var newHandle = await AddressableManager.LoadAsync<T>(key, progress, cancellationToken);

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
    }
}
