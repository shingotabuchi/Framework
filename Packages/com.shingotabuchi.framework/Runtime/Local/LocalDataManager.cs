namespace Fwk.Local
{
    public class LocalDataManager : SingletonGeneric<LocalDataManager>
    {
        public LocalDataProvider Provider = new();
    }
}