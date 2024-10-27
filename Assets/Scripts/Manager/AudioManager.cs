using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : UnitySingleton<AudioManager>
{
    [Header("UI Audio")]
    //UI��Ч�����
    public int AudioPoolSize = 10;
    private GameObject audioSourceParent;
    private Queue<AudioSource> audioSources;

    //UI������
    public AudioMixerGroup uiAudioMixerGroup;
    private float initUIAudioVolume;

    //����UI��Ƶ�б�
    [System.Serializable]
    public class UIAudioClipConfig
    {
        public UIAudioType type;
        public AudioClip clip;
    }
    public List<UIAudioClipConfig> uiAudioClips;
    private Dictionary<UIAudioType, AudioClip> uiAudioClipsDict = new Dictionary<UIAudioType, AudioClip>();


    [Header("BGM")]
    //���������飨����ʹ�õ�����AudioSource��
    public AudioMixerGroup bgmAudioMixerGroup;
    private float initBGMAudioVolume;
    private AudioSource bgmAudioSource;

    //���ñ�����Ƶ�б�
    [System.Serializable]
    public class BGAudioClipConfig
    {
        public string SongTitle, Singer;
        public AudioClip song;
    }
    public List<BGAudioClipConfig> BGMs;
    private int currentBgmIndex;
    private bool isMusicPlay = true;
    

    //ί��
    public event System.Action<BGAudioClipConfig> OnBGMSelected;
    public event System.Action OnBGMPlay, OnBGMPause;


    protected override void Awake()
    {
        base.Awake();

        //��ʼ��Ĭ�ϲ���
        uiAudioMixerGroup.audioMixer.GetFloat("UIVolume", out initUIAudioVolume);
        bgmAudioMixerGroup.audioMixer.GetFloat("BGMVolume", out initBGMAudioVolume);

        //�༭��������õ�UI��������תΪ�ֵ�
        foreach(var config in  uiAudioClips)
        {
            uiAudioClipsDict.Add(config.type, config.clip);
        }
        uiAudioClips.Clear();

        //��ʼ��һ��AudioSources�Ӽ�����
        audioSourceParent = transform.Find("AudioSources")?.gameObject;
        if (audioSourceParent == null)
        {
            GameObject obj = new GameObject("AudioSources");
            audioSourceParent = obj;
            audioSourceParent.transform.parent = transform;
        }

        //���Ӽ����������UIAudioSource
        audioSources = new Queue<AudioSource>(AudioPoolSize);
        for (int i = 0; i < AudioPoolSize; i++)
        {
            audioSources.Enqueue(audioSourceParent.AddComponent<AudioSource>());
        }

        //���Ӽ�����ӱ�����AudioSource
        bgmAudioSource = audioSourceParent.AddComponent<AudioSource>();
        bgmAudioSource.outputAudioMixerGroup = bgmAudioMixerGroup;
    }

    private void Start()
    {
        //��ȡUI��Ƶ����
        SetUIAudioVolume(SaveManager.Instance.data.uiVolumeMultiper);
        //��ȡ������Ƶ����
        SetBGMAudioVolume(SaveManager.Instance.data.bgmVolumeMultiper);

        //��ȡ��������
        isMusicPlay = SaveManager.Instance.data.isMusicPlay;

        //���ѡ�񱳾�����
        SelectBGM(Random.Range(0, BGMs.Count));
    }

    //����UI����
    public void SetUIAudioVolume(float multiper)
    {
        //�޸�AudioMixerGroup
        uiAudioMixerGroup.audioMixer.SetFloat("UIVolume", multiper < 0.05 ? -1000 : Mathf.LerpUnclamped(initUIAudioVolume * 2, initUIAudioVolume, multiper));

        //�洢������
        SaveManager.Instance.data.uiVolumeMultiper = multiper;
        SaveManager.Instance.Save();
    }

    //����BGM����
    public void SetBGMAudioVolume(float multiper)
    {
        //�޸�AudioMixerGroup
        bgmAudioMixerGroup.audioMixer.SetFloat("BGMVolume", multiper < 0.05 ? -1000 : Mathf.LerpUnclamped(initBGMAudioVolume * 2, initBGMAudioVolume, multiper));

        //�洢������
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