using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalLightEntryUI : MonoBehaviour
{
    public Button resetBtn;
    public Slider HorizontalSlider, VerticalSlider;
    private float initHorizontalAngle, initVerticalAngle;

    private void Awake()
    {
        //ΪĬ�ϲ�����ֵ
        initHorizontalAngle = HorizontalSlider.value;
        initVerticalAngle = VerticalSlider.value;

        //�󶨰�ťί��
        resetBtn.onClick.AddListener(() =>
        {
            HorizontalSlider.value = initHorizontalAngle;
            VerticalSlider.value = initVerticalAngle;
        });

        //�󶨻�����ί��
        HorizontalSlider.onValueChanged.AddListener((currentValue) =>
        {
            GameManager.Instance.SetLightAngleHorizontal(currentValue);
            Save();
        });

        VerticalSlider.onValueChanged.AddListener((currentValue) =>
        {
            GameManager.Instance.SetLightAngleVertical(currentValue);
            Save();
        });
    }

    private void Start()
    {
        HorizontalSlider.value = SaveManager.Instance.data.lightHorizontalAngle;
        VerticalSlider.value = SaveManager.Instance.data.lightVerticalAngle;
    }

    void Save()
    {
        SaveManager.Instance.data.lightHorizontalAngle = HorizontalSlider.value;
        SaveManager.Instance.data.lightVerticalAngle = VerticalSlider.value;
        SaveManager.Instance.Save();
    }
}
