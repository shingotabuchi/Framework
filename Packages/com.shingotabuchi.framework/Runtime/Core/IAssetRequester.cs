using Cysharp.Threading.Tasks;
using System.Threading;

namespace Fwk
{
    public interface IAssetRequester
    {
        UniTask<T> RequestAsset<T>(
            string key,
            CancellationToken cancellationToken = default
        ) where T : UnityEngine.Object;
    }
}