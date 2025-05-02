namespace Fwk.Local
{
    public class LocalDataProvider : SingletonGeneric<JsonDataProvider<LocalDataProvider>>
    {
        public LocalData LocalData;
    }
}