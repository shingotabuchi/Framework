using UnityEngine;
using System.Collections.Generic;
using Fwk.Addressables;

namespace Fwk.Sound
{
    public class SoundManager : SingletonPersistent<SoundManager>
    {
        private AddressableCache addressableCache = new();
        private Dictionary<string, ISoundCueSheet> cueSheetDict = new();
    }
}
