using UnityEngine;

namespace Fwk
{
    public class Singleton
    {
        private static Singleton _instance = new Singleton();

        public static Singleton Instance
        {
            get
            {
                return _instance;
            }
        }

        protected Singleton()
        {
            if (_instance != null)
                throw new System.InvalidOperationException("Only one Singleton allowed!");
            _instance = this;
        }
    }

    public class Singleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }
        public static bool Exists { get => Instance != null; }
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public class SingletonPersistent<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}