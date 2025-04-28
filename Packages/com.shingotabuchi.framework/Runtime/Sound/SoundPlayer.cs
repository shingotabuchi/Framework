using UnityEngine;

namespace Fwk.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void PlayOneShot(ISoundData soundData, float volume = 1.0f)
        {
            if (soundData == null || soundData.Clip == null) return;
            _audioSource.PlayOneShot(soundData.Clip, soundData.Volume * volume);
        }
    }
}