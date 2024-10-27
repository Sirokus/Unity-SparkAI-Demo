using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    //面板控制
    public Button openBtn, closeBtn;
    public GameObject settingsPage;

    //设置字段
    //大模型
    public TMP_Dropdown dropDown_Spark;

    //系统设置
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
        //从保存的数据中读取配置，实际数据已经在对应Manager中读取，这里只需要让UI对上就行
        //所以不要触发委托或者在委托前执行
        dropDown_Spark.value = SaveManager.Instance.data.LLMSparkType;
        slider_UIVolume.value = SaveManager.Instance.data.uiVolumeMultiper;
        slider_BGMVolume.value = SaveManager.Instance.data.bgmVolumeMultiper;


        //绑定委托
        dropDown_Spark.onValueChanged.AddListener((selectedIndex) =>
        {
            AIManager.Instance.SelectLLMType((AIManager.LLMType)selectedIndex);
            SaveManager.Instance.data.LLMSparkType = selectedIndex;

            //防止污染，清空对话记录
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
