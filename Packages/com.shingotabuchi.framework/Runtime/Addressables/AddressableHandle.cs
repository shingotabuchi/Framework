using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fwk.Addressables
{
    public class AddressableHandle<T> : IAddressableHandle where T : UnityEngine.Object
    {
        private readonly AsyncOperationHandle<T> _handle;

        public AddressableHandle(AsyncOperationHandle<T> handle)
        {
            _handle = handle;
        }

        public UnityEngine.Object Asset => _handle.Result;

        public AsyncOperationStatus Status => _handle.Status;

        public void Release()
        {
            if (_handle.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(_handle);
            }
        }
    }
}
