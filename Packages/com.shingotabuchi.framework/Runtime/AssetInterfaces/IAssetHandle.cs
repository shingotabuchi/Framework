namespace Fwk
{
    public interface IAssetHandle<T> where T : UnityEngine.Object
    {
        T Asset { get; }
        void Release();
    }
}