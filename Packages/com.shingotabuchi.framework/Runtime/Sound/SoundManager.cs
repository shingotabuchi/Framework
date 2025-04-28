using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fwk.Addressables;
using Cysharp.Threading.Tasks;

namespace Fwk.Sound
{
    public class SoundManager : SingletonPersistent<SoundManager>
    {
        private AddressableCache addressableCache = new();
        private Dictionary<string, ISoundCueSheet> cueSheetDict = new();
        private Dictionary<string, ISoundData> soundDataDict = new();
        private HashSet<string> loadingCueSheets = new();
        private Dictionary<SoundType, HashSet<SoundPlayer>> _players = new();
        private Dictionary<SoundType, HashSet<SoundPlayer>> _playingPlayers = new();

        protected override void Awake()
        {
            base.Awake();
            InitializePlayers();
        }

        private void InitializePlayers()
        {
            _players[SoundType.SE] = new HashSet<SoundPlayer>();
            _players[SoundType.BGM] = new HashSet<SoundPlayer>();
            CreateNewPlayer(SoundType.SE);
            CreateNewPlayer(SoundType.BGM);
            CreateNewPlayer(SoundType.BGM);
        }

        private void CreateNewPlayer(SoundType type)
        {
            var player = new GameObject($"SoundPlayer_{type}_{_players[type].Count}").AddComponent<SoundPlayer>();
            player.transform.SetParent(transform);
            _players[type].Add(player);
        }

        public void PlaySeOneShot(string soundName, float volume = 1.0f)
        {
            if (!soundDataDict.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found.");
                return;
            }
            var soundData = soundDataDict[soundName];
            var player = _players[SoundType.SE].FirstOrDefault();
            if (player == null)
            {
                Debug.LogWarning($"No available sound player for SE.");
                return;
            }
            player.PlayOneShot(soundData, volume);
        }

        public async UniTask LoadCueSheetAsync(string cueSheetName, CancellationToken cancellationToken = default, IProgress<float> progress = null)
        {
            if (cueSheetDict.ContainsKey(cueSheetName))
            {
                Debug.LogWarning($"Cue sheet '{cueSheetName}' is already loaded.");
                return;
            }

            if (loadingCueSheets.Contains(cueSheetName))
            {
                Debug.LogWarning($"Cue sheet '{cueSheetName}' is already loading.");
                return;
            }

            loadingCueSheets.Add(cueSheetName);
            try
            {
                cueSheetDict[cueSheetName] = await addressableCache.LoadAsync<SoundCueSheet>(
                    AddressableAssetKeys.GetCueSheetKey(cueSheetName), cancellationToken, progress);
                AddSoundDataFromCueSheet(cueSheetDict[cueSheetName]);
            }
            finally
            {
                loadingCueSheets.Remove(cueSheetName);
            }
        }

        public void UnloadCueSheet(string cueSheetName)
        {
            if (!cueSheetDict.ContainsKey(cueSheetName))
            {
                Debug.LogWarning($"Cue sheet '{cueSheetName}' is not loaded.");
                return;
            }

            RemoveSoundDataFromCueSheet(cueSheetDict[cueSheetName]);
            cueSheetDict.Remove(cueSheetName);

            addressableCache.Release(AddressableAssetKeys.GetCueSheetKey(cueSheetName));
        }

        private void AddSoundDataFromCueSheet(ISoundCueSheet cueSheet)
        {
            foreach (var soundData in cueSheet.SoundDatas)
            {
                if (!soundDataDict.ContainsKey(soundData.Name))
                {
                    soundDataDict[soundData.Name] = soundData;
                }
            }
        }

        private void RemoveSoundDataFromCueSheet(ISoundCueSheet cueSheet)
        {
            foreach (var soundData in cueSheet.SoundDatas)
            {
                if (soundDataDict.ContainsKey(soundData.Name))
                {
                    soundDataDict.Remove(soundData.Name);
                }
            }
        }
    }
}
