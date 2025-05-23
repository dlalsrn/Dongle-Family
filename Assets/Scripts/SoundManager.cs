using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    Attach,
    Button,
    GameOver,
    LevelUp,
    LevelUpB,
    LevelUpC,
    Next,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("# BGM")]
    private GameObject bgmPlayer;
    private AudioSource bgmAudioSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private float bgmVolume;

    [Header("# SFX")]
    private GameObject sfxPlayer;
    private List<AudioSource> sfxPlayerList = new List<AudioSource>();
    [SerializeField] private AudioClip[] sfxClip;
    [SerializeField] private float sfxVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Init();
    }

    private void Init()
    {
        // BGM 초기화
        bgmPlayer = new GameObject("BgmPlayer");
        bgmPlayer.transform.parent = transform;
        bgmAudioSource = bgmPlayer.AddComponent<AudioSource>();
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.clip = bgmClip;

        // SFX 초기화
        sfxPlayer = new GameObject("SfxPlayer");
        sfxPlayer.transform.parent = transform;
    }

    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
        {
            bgmAudioSource.Play();
        }
        else
        {
            bgmAudioSource.Stop();
        }
    }

    public void PlaySfx(Sfx sfx)
    {
        AudioSource audioSource = GetAudioSource();

        int ranIndex = 0;

        if (sfx == Sfx.LevelUp)
        {
            ranIndex = Random.Range(0, 3);
        }

        audioSource.clip = sfxClip[(int)sfx + ranIndex];
        audioSource.Play();
    }

    private AudioSource GetAudioSource()
    {
        foreach (AudioSource sfxPlayer in sfxPlayerList)
        {
            // 쉬고 있는 SfxPlayer 선택
            if (!sfxPlayer.isPlaying)
            {
                return sfxPlayer;
            }
        }

        // 만약 모든 Player가 재생 중이라면 새로 생성
        return CreateAudioSource();
    }

    private AudioSource CreateAudioSource()
    {
        AudioSource audioSource = sfxPlayer.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = sfxVolume;
        sfxPlayerList.Add(audioSource);

        return audioSource;
    }
}
