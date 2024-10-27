using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : UnitySingleton<AudioManager>
{
    [Header("UI Audio")]
    //UI音效对象池
    public int AudioPoolSize = 10;
    private GameObject audioSourceParent;
    private Queue<AudioSource> audioSources;

    //UI音量组
    public AudioMixerGroup uiAudioMixerGroup;
    private float initUIAudioVolume;

    //配置UI音频列表
    [System.Serializable]
    public class UIAudioClipConfig
    {
        public UIAudioType type;
        public AudioClip clip;
    }
    public List<UIAudioClipConfig> uiAudioClips;
    private Dictionary<UIAudioType, AudioClip> uiAudioClipsDict = new Dictionary<UIAudioType, AudioClip>();


    [Header("BGM")]
    //背景音量组（背景使用单独的AudioSource）
    public AudioMixerGroup bgmAudioMixerGroup;
    private float initBGMAudioVolume;
    private AudioSource bgmAudioSource;

    //配置背景音频列表
    [System.Serializable]
    public class BGAudioClipConfig
    {
        public string SongTitle, Singer;
        public AudioClip song;
    }
    public List<BGAudioClipConfig> BGMs;
    private int currentBgmIndex;
    private bool isMusicPlay = true;
    

    //委托
    public event System.Action<BGAudioClipConfig> OnBGMSelected;
    public event System.Action OnBGMPlay, OnBGMPause;


    protected override void Awake()
    {
        base.Awake();

        //初始化默认参数
        uiAudioMixerGroup.audioMixer.GetFloat("UIVolume", out initUIAudioVolume);
        bgmAudioMixerGroup.audioMixer.GetFloat("BGMVolume", out initBGMAudioVolume);

        //编辑器面板配置的UI参数数组转为字典
        foreach(var config in  uiAudioClips)
        {
            uiAudioClipsDict.Add(config.type, config.clip);
        }
        uiAudioClips.Clear();

        //初始化一个AudioSources子级对象
        audioSourceParent = transform.Find("AudioSources")?.gameObject;
        if (audioSourceParent == null)
        {
            GameObject obj = new GameObject("AudioSources");
            audioSourceParent = obj;
            audioSourceParent.transform.parent = transform;
        }

        //在子级对象上添加UIAudioSource
        audioSources = new Queue<AudioSource>(AudioPoolSize);
        for (int i = 0; i < AudioPoolSize; i++)
        {
            audioSources.Enqueue(audioSourceParent.AddComponent<AudioSource>());
        }

        //在子级上添加背景的AudioSource
        bgmAudioSource = audioSourceParent.AddComponent<AudioSource>();
        bgmAudioSource.outputAudioMixerGroup = bgmAudioMixerGroup;
    }

    private void Start()
    {
        //读取UI音频设置
        SetUIAudioVolume(SaveManager.Instance.data.uiVolumeMultiper);
        //读取背景音频设置
        SetBGMAudioVolume(SaveManager.Instance.data.bgmVolumeMultiper);

        //读取播放设置
        isMusicPlay = SaveManager.Instance.data.isMusicPlay;

        //随机选择背景音乐
        SelectBGM(Random.Range(0, BGMs.Count));
    }

    //设置UI音量
    public void SetUIAudioVolume(float multiper)
    {
        //修改AudioMixerGroup
        uiAudioMixerGroup.audioMixer.SetFloat("UIVolume", multiper < 0.05 ? -1000 : Mathf.LerpUnclamped(initUIAudioVolume * 2, initUIAudioVolume, multiper));

        //存储到本地
        SaveManager.Instance.data.uiVolumeMultiper = multiper;
        SaveManager.Instance.Save();
    }

    //设置BGM音量
    public void SetBGMAudioVolume(float multiper)
    {
        //修改AudioMixerGroup
        bgmAudioMixerGroup.audioMixer.SetFloat("BGMVolume", multiper < 0.05 ? -1000 : Mathf.LerpUnclamped(initBGMAudioVolume * 2, initBGMAudioVolume, multiper));

        //存储到本地
        SaveManager.Instance.data.bgmVolumeMultiper = multiper;
        SaveManager.Instance.Save();
    }


    public void PlayAudio(AudioClip audioClip, AudioSource audioSource)
    {
        audioSource.PlayOneShot(audioClip);
        audioSources.Enqueue(audioSource);
    }

    public void PlayUIAudio(UIAudioType audioType)
    {
        AudioSource audioSource = audioSources.Dequeue();
        audioSource.outputAudioMixerGroup = uiAudioMixerGroup;
        PlayAudio(uiAudioClipsDict[audioType], audioSource);
    }

    private void Update()
    {
        if(isMusicPlay)
        {
            if(!bgmAudioSource.isPlaying)
            {
                SelectNextBGM();
            }
        }
    }

    //BGM
    public void PlayBGM()
    {
        isMusicPlay = true;

        bgmAudioSource.clip = BGMs[currentBgmIndex].song;
        bgmAudioSource.Play();

        SaveManager.Instance.data.isMusicPlay = true;
        SaveManager.Instance.Save();

        OnBGMPlay?.Invoke();
    }

    public void PauseBGM()
    {
        isMusicPlay = false;

        bgmAudioSource.Pause();

        SaveManager.Instance.data.isMusicPlay = false;
        SaveManager.Instance.Save();

        OnBGMPause?.Invoke();
    }

    public void SelectBGM(int index)
    {
        if (index >= BGMs.Count)
            return;

        currentBgmIndex = index;
        OnBGMSelected?.Invoke(BGMs[index]);

        if (isMusicPlay)
            PlayBGM();
        else
            PauseBGM();
    }

    public void SelectLastBGM() => SelectBGM((currentBgmIndex - 1) % BGMs.Count);
    public void SelectNextBGM() => SelectBGM((currentBgmIndex + 1) % BGMs.Count);
}

public enum UIAudioType
{
    DialoguePopText,
    SendBtn,
    ButtonPress
}