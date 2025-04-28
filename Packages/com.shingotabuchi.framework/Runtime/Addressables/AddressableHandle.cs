using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fwk
{
    public class AddressableHandle<T> where T : UnityEngine.Object
    {
        private readonly AsyncOperationHandle<T> _handle;

        public AddressableHandle(AsyncOperationHandle<T> handle)
        {
            _handle = handle;
        }

        public T Asset => _handle.Result;

        public void Release()
        {
            if (_handle.IsValid())
            {
                Addressables.Release(_handle);
            }
        }
    }
}
