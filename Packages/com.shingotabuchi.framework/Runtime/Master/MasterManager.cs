using System.Threading;
using System.Collections.Generic;
using System;
using Fwk.Addressables;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Fwk.Master
{
    public class MasterManager : SingletonPersistent<MasterManager>
    {
        private Dictionary<Type, IReadOnlyList<IMasterData>> _masterData = new Dictionary<Type, IReadOnlyList<IMasterData>>();
        private bool _isInitialized = false;
        private bool _isInitializing = false;

        public static void CreateIfNotExists()
        {
            if (Instance != null)
            {
                return;
            }
            var go = new GameObject("MasterManager");
            Instance = go.AddComponent<MasterManager>();
        }

        public async UniTask Initialize(CancellationToken token)
        {
            while (true)
            {
                if (_isInitialized)
                {
                    return;
                }
                if (_isInitializing)
                {
                    await UniTask.Yield(token);
                    continue;
                }
                _isInitializing = true;
                try
                {
                    await InitializeInternal(token);
                }
                finally
                {
                    _isInitializing = false;
                }
                break;
            }
        }

        private async UniTask InitializeInternal(CancellationToken token)
        {
            var handle = await AddressableManager.LoadByLabelAsync<ScriptableObject>("Master", cancellationToken: token);
            if (!handle.Succeeded)
            {
                Debug.LogError($"Failed to load master data");
                return;
            }
            if (handle.Objects == null || handle.Objects.Count == 0)
            {
                Debug.LogError($"No master data found");
                return;
            }

            foreach (var masterData in handle.Objects)
            {
                if (masterData is IMasterDataScriptableObject masterDataSO)
                {
                    if (_masterData.ContainsKey(masterDataSO.Type))
                    {
                        Debug.LogError($"Master data of type {masterDataSO.Type} already exists. Skipping loading.");
                        continue;
                    }

                    var data = masterDataSO.Data;
                    for (int i = 0; i < data.Count; i++)
                    {
                        data[i].Id = i + 1;
                    }

                    _masterData.Add(masterDataSO.Type, masterDataSO.Data);
                }
                else
                {
                    Debug.LogError($"Master data of type {masterData.GetType()} is not a valid IMasterDataScriptableObject. Skipping loading.");
                }
            }
            handle.Release();
            _isInitialized = true;
        }

        public IReadOnlyList<T> GetMasterData<T>() where T : IMasterData
        {
            if (_masterData.TryGetValue(typeof(T), out var data))
            {
                return (IReadOnlyList<T>)data;
            }
            Debug.LogError($"Master data of type {typeof(T)} not found.");
            return null;
        }

        public T GetMasterDataById<T>(int id) where T : IMasterData
        {
            var data = GetMasterData<T>();
            if (data == null)
            {
                Debug.LogError($"Master data of type {typeof(T)} not found.");
                return default;
            }
            if (id < 1 || id > data.Count)
            {
                Debug.LogError($"Master data of type {typeof(T)} with id {id} not found.");
                return default;
            }
            return data[id - 1];
        }
    }
}