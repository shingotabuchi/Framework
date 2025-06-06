using System;
using UnityEngine;

namespace Fwk.Sound
{
    [Serializable]
    public class SoundData : ISoundData
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField] private float _volume = 1.0f;
        public string Name => _clip.name;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public float PlayedVolume { get; set; } = 1.0f;
    }
}