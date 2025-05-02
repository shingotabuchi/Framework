namespace Fwk.Local
{
    public class JsonDataProvider<T> : Singleton where T : class, new()
    {
        private JsonDataManager<T> _dataManager = new JsonDataManager<T>();
        private T _data;
        public T GetData()
        {
            if (_data == null)
            {
                return Load();
            }
            return _data;
        }
        public T Load()
        {
            _data = _dataManager.Load();
            return _data;
        }
        public void Save()
        {
            _dataManager.Save(_data);
        }
        public void Delete()
        {
            _dataManager.Delete();
            _data = null;
        }
        public bool Exists()
        {
            return _dataManager.Exists();
        }
    }
}