using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    //������
    public Button openBtn, closeBtn;
    public GameObject settingsPage;

    //�����ֶ�
    //��ģ��
    public TMP_Dropdown dropDown_Spark;

    //ϵͳ����
    public Slider slider_UIVolume;
    public Slider slider_BGMVolume;

    private void Awake()
    {
        openBtn.onClick.AddListener(() =>
        {
            settingsPage.SetActive(true);
            AudioManager.Instance.PlayUIAudio(UIAudioType.ButtonPress);
        });
        closeBtn.onClick.AddListener(() => settingsPage.SetActive(false));
    }

    // Start is called before the first frame update
    void Start()
    {
        //�ӱ���������ж�ȡ���ã�ʵ�������Ѿ��ڶ�ӦManager�ж�ȡ������ֻ��Ҫ��UI���Ͼ���
        //���Բ�Ҫ����ί�л�����ί��ǰִ��
        dropDown_Spark.value = SaveManager.Instance.data.LLMSparkType;
        slider_UIVolume.value = SaveManager.Instance.data.uiVolumeMultiper;
        slider_BGMVolume.value = SaveManager.Instance.data.bgmVolumeMultiper;


        //��ί��
        dropDown_Spark.onValueChanged.AddListener((selectedIndex) =>
        {
            AIManager.Instance.SelectLLMType((AIManager.LLMType)selectedIndex);
            SaveManager.Instance.data.LLMSparkType = selectedIndex;

            //��ֹ��Ⱦ����նԻ���¼
            DialogueManager.Instance.ClearHistory();
        });

        slider_UIVolume.onValueChanged.AddListener((currentMultiper) =>
        {
            AudioManager.Instance.SetUIAudioVolume(currentMultiper);
        });

        slider_BGMVolume.onValueChanged.AddListener((currentMultiper) =>
        {
            AudioManager.Instance.SetBGMAudioVolume(currentMultiper);
        });

        
    }
}
