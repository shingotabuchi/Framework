using System;
using UnityEngine;
using UnityEngine.UI;
using Fwk.Sound;
using Cysharp.Threading.Tasks;

public class SoundTest : MonoBehaviour
{
    [SerializeField] SeTestData[] seTestDatas;
    [SerializeField] int seIndex = 0;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;
    [SerializeField] Button playButton;
    [SerializeField] SeTestData bgmTestData;

    private void Start()
    {
        SoundManager.Instance.LoadCueSheetAsync("SE").Forget();
        SoundManager.Instance.LoadCueSheetAsync("BGM").Forget();

        nextButton.onClick.AddListener(NextSe);
        prevButton.onClick.AddListener(PrevSe);
        playButton.onClick.AddListener(PlaySe);
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

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void PlayBgm()
    {
        bgmTestData.PlayBgm();
    }

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void PauseBgm()
    {
        SoundManager.Instance.PauseBgm(0);
    }

#if UNITY_EDITOR
    [Fwk.Editor.Button]
#endif
    public void ResumeBgm()
    {
        SoundManager.Instance.ResumeBgm(0);
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

    public void PlayBgm()
    {
        SoundManager.Instance.PlayBgm(soundName).Forget();
    }
}