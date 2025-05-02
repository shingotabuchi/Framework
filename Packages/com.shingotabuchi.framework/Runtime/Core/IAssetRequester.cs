using Cysharp.Threading.Tasks;
using System.Threading;

public interface IAssetRequester
{
    UniTask<T> RequestAsset<T>(
        string key,
        CancellationToken cancellationToken = default
    ) where T : UnityEngine.Object;
}