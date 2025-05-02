namespace Fwk.Local
{
    public class LocalDataManager : SingletonGeneric<LocalDataManager>
    {
        public LocalData LocalData => _provider.LocalData;
        private LocalDataProvider _provider = new();

        public void Save()
        {
            _provider.Save();
        }

        public void Load()
        {
            _provider.Load();
        }

        public void DeleteFile()
        {
            _provider.DeleteFile();
        }

        public bool FileExists()
        {
            return _provider.FileExists();
        }
    }
}