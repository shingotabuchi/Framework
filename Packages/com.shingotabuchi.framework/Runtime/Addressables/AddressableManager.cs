using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Fwk.Addressables
{
    public static class AddressableManager
    {
        public static async UniTask<AddressableHandle<T>> LoadAsync<T>(
            string key,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        ) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            AsyncOperationHandle<T> handle = new();
            try
            {
                handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(key);
                while (!handle.IsDone)
                {
                    progress?.Report(handle.PercentComplete);
                    await UniTask.Yield(cancellationToken);
                }

                progress?.Report(1f);
                return new AddressableHandle<T>(handle);
            }
            catch (OperationCanceledException)
            {
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load addressable asset with key '{key}': {ex}");
                if (handle.IsValid())
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                }
                throw;
            }
        }
    }
}
