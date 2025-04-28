using System;
using UnityEngine;
using Fwk.Sound;
using Cysharp.Threading.Tasks;

public class SoundTest : MonoBehaviour
{
    [SerializeField] SeTestData[] seTestDatas;
    [SerializeField] int seIndex = 0;

    private void Start()
    {
        SoundManager.Instance.LoadCueSheetAsync("SE").Forget();
    }

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void NextSe()
    {
        seIndex = (seIndex + 1) % seTestDatas.Length;
    }

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void PrevSe()
    {
        seIndex = (seIndex - 1 + seTestDatas.Length) % seTestDatas.Length;
    }

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void PlaySe()
    {
        seIndex = Mathf.Clamp(seIndex, 0, seTestDatas.Length - 1);
        if (seTestDatas.Length > 0)
        {
            seTestDatas[seIndex].PlaySe();
        }
    }
}

[Serializable]
public class SeTestData
{
    public string soundName;
    public float volume = 1.0f;

    public void PlaySe()
    {
        SoundManager.Instance.PlaySeOneShot(soundName, volume);
    }
}