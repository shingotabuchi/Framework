using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Fwk.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _audioSource0;
        private AudioSource _audioSource1;
        private bool _isCrossfading;
        private CancellationTokenSource _crossfadeCts;

        private void Awake()
        {
            _audioSource0 = GetComponent<AudioSource>();
            _audioSource0.playOnAwake = false;
            _audioSource0.loop = false;

            // Create crossfade source
            var crossfadeObj = new GameObject("CrossfadeSource");
            crossfadeObj.transform.SetParent(transform);
            _audioSource1 = crossfadeObj.AddComponent<AudioSource>();
            _audioSource1.playOnAwake = false;
            _audioSource1.loop = false;
        }

        private void OnDestroy()
        {
            _crossfadeCts?.Cancel();
            _crossfadeCts?.Dispose();
        }

        public void PlayOneShot(ISoundData soundData, float volume = 1.0f)
        {
            if (soundData == null || soundData.Clip == null) return;
            _audioSource0.PlayOneShot(soundData.Clip, soundData.Volume * volume);
        }

        public void PlayBgm(ISoundData soundData, float volume = 1.0f)
        {
            if (soundData == null || soundData.Clip == null) return;

            _audioSource0.clip = soundData.Clip;
            _audioSource0.volume = soundData.Volume * volume;
            _audioSource0.loop = true;
            _audioSource0.Play();
        }

        public async UniTask CrossfadeBgm(ISoundData newSoundData, float duration = 1.0f, float volume = 1.0f)
        {
            if (newSoundData == null || newSoundData.Clip == null) return;

            // Cancel any ongoing crossfade
            _crossfadeCts?.Cancel();
            _crossfadeCts?.Dispose();
            _crossfadeCts = new CancellationTokenSource();

            // Wait for any existing crossfade to complete
            while (_isCrossfading)
            {
                await UniTask.Yield();
            }

            _isCrossfading = true;

            var playingSource = _audioSource0.isPlaying ? _audioSource0 : _audioSource1;
            var nonPlayingSource = _audioSource0.isPlaying ? _audioSource1 : _audioSource0;

            // Setup crossfade source
            nonPlayingSource.clip = newSoundData.Clip;
            nonPlayingSource.volume = 0f;
            nonPlayingSource.loop = true;
            nonPlayingSource.Play();

            var startTime = Time.time;
            var startVolume = playingSource.volume;
            var endVolume = newSoundData.Volume * volume;

            try
            {
                while (Time.time - startTime < duration)
                {
                    float t = (Time.time - startTime) / duration;
                    playingSource.volume = Mathf.Lerp(startVolume, 0f, t);
                    nonPlayingSource.volume = Mathf.Lerp(0f, endVolume, t);
                    await UniTask.Yield(_crossfadeCts.Token);
                }

                // Final state
                playingSource.Stop();
                // _audioSource0.clip = newSoundData.Clip;
                // _audioSource0.volume = endVolume;
                // _audioSource0.Play();
                // _audioSource1.Stop();
            }
            catch (System.OperationCanceledException)
            {
                // Handle cancellation
            }
            finally
            {
                _isCrossfading = false;
            }
        }

        public void StopBGM()
        {
            _audioSource0.Stop();
            _audioSource1.Stop();
            _crossfadeCts?.Cancel();
        }

        public bool IsPlaying => _audioSource0.isPlaying || _audioSource1.isPlaying;
    }
}