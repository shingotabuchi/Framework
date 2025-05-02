using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fwk.Addressables
{
    public class AddressableHandle<T> : IAddressableHandle
    {
        private readonly AsyncOperationHandle<T> _handle;
        public AddressableHandle(AsyncOperationHandle<T> handle)
        {
            _handle = handle;
        }

        public UnityEngine.Object Object => _handle.Result as UnityEngine.Object;
        public IReadOnlyList<UnityEngine.Object> Objects => _handle.Result as IReadOnlyList<UnityEngine.Object>;
        public AsyncOperationStatus Status => _handle.Status;

        public void Release()
        {
            if (_handle.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(_handle);
            }
        }

        public T GetResult()
        {
            if (_handle.Status == AsyncOperationStatus.Succeeded)
            {
                return _handle.Result;
            }
            else
            {
                throw new InvalidOperationException($"Handle is not valid. Status: {_handle.Status}");
            }
        }
    }
}
