using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Fwk
{
    public class AddressableCache
    {
        private readonly Dictionary<string, object> _handles = new();

        public async UniTask<T> LoadAsync<T>(string key, IProgress<float> progress = null) where T : UnityEngine.Object
        {
            if (_handles.TryGetValue(key, out var boxedHandle) && boxedHandle is IAssetHandle<T> existingHandle)
            {
                return existingHandle.Asset;
            }

            var newHandle = await AddressableManager.LoadAsync<T>(key, progress);
            _handles[key] = newHandle;

            return newHandle.Asset;
        }

        public void Release(string key)
        {
            if (_handles.TryGetValue(key, out var boxedHandle))
            {
                switch (boxedHandle)
                {
                    case IAssetHandle<UnityEngine.Object> handle:
                        handle.Release();
                        break;
                }
                _handles.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var boxedHandle in _handles.Values)
            {
                if (boxedHandle is IAssetHandle<UnityEngine.Object> handle)
                {
                    handle.Release();
                }
            }
            _handles.Clear();
        }
    }
}
