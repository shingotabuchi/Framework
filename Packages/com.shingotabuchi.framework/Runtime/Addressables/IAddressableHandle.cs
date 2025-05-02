using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fwk.Addressables
{
    public interface IAddressableHandle
    {
        Object Object { get; }
        IReadOnlyList<Object> Objects { get; }
        AsyncOperationStatus Status { get; }
        void Release();
    }
}
