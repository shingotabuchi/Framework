using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Fwk
{
    public static class AddressableManager
    {
        public static async UniTask<AddressableHandle<T>> LoadAsync<T>(string key, IProgress<float> progress = null) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);

            while (!handle.IsDone)
            {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield();
            }

            progress?.Report(1f);
            return new AddressableHandle<T>(handle);
        }
    }
}
