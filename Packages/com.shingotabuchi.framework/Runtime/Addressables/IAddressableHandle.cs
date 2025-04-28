using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fwk.Addressables
{
    public interface IAddressableHandle
    {
        UnityEngine.Object Asset { get; }
        AsyncOperationStatus Status { get; }
        void Release();
    }
}
