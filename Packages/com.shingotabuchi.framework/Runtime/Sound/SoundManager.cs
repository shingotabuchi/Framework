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
        private Dictionary<int, SoundPlayer> _bgmChannels = new();

        public static void CreateIfNotExists()
        {
            if (Instance == null)
            {
                Instance = new GameObject("SoundManager").AddComponent<SoundManager>();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitializePlayers();
        }

        private void InitializePlayers()
        {
            _players[SoundType.SE] = new HashSet<SoundPlayer>();
            _players[SoundType.BGM] = new HashSet<SoundPlayer>();
            _playingPlayers[SoundType.SE] = new HashSet<SoundPlayer>();
            _playingPlayers[SoundType.BGM] = new HashSet<SoundPlayer>();

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

        public async UniTask PlayBgm(string soundName, int channel = 0, float volume = 1.0f, float crossfadeDuration = 1.0f)
        {
            if (!soundDataDict.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found.");
                return;
            }

            var soundData = soundDataDict[soundName];
            var player = GetOrCreateBGMPlayer(channel);
            if (player == null)
            {
                Debug.LogWarning($"Failed to get or create BGM player for channel {channel}.");
                return;
            }

            await player.CrossfadeBgm(soundData, crossfadeDuration, volume);
            _playingPlayers[SoundType.BGM].Add(player);
        }

        public void PlayBGMImmediate(string soundName, int channel = 0, float volume = 1.0f)
        {
            if (!soundDataDict.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found.");
                return;
            }

            var soundData = soundDataDict[soundName];
            var player = GetOrCreateBGMPlayer(channel);
            if (player == null)
            {
                Debug.LogWarning($"Failed to get or create BGM player for channel {channel}.");
                return;
            }

            player.PlayBgm(soundData, volume);
            _playingPlayers[SoundType.BGM].Add(player);
        }

        public void StopBGM(int channel = 0)
        {
            if (_bgmChannels.TryGetValue(channel, out var player))
            {
                player.StopBGM();
                _playingPlayers[SoundType.BGM].Remove(player);
            }
        }

        public void StopAllBGM()
        {
            foreach (var player in _playingPlayers[SoundType.BGM])
            {
                player.StopBGM();
            }
            _playingPlayers[SoundType.BGM].Clear();
        }

        private SoundPlayer GetOrCreateBGMPlayer(int channel)
        {
            if (_bgmChannels.TryGetValue(channel, out var existingPlayer))
            {
                return existingPlayer;
            }

            var availablePlayer = _players[SoundType.BGM].FirstOrDefault(p => !p.IsPlaying);
            if (availablePlayer == null)
            {
                CreateNewPlayer(SoundType.BGM);
                availablePlayer = _players[SoundType.BGM].Last();
            }

            _bgmChannels[channel] = availablePlayer;
            return availablePlayer;
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
