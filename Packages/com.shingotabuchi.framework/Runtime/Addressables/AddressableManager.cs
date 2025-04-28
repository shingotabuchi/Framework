using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Fwk
{
    public static class AddressableManager
    {
        public static async UniTask<AddressableHandle<T>> LoadAsync<T>(
            string key,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default
        ) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);

            try
            {
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
                    Addressables.Release(handle);
                }
                throw;
            }
        }
    }
}
