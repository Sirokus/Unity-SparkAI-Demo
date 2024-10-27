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
        //为默认参数赋值
        initHorizontalAngle = HorizontalSlider.value;
        initVerticalAngle = VerticalSlider.value;

        //绑定按钮委托
        resetBtn.onClick.AddListener(() =>
        {
            HorizontalSlider.value = initHorizontalAngle;
            VerticalSlider.value = initVerticalAngle;
        });

        //绑定滑动条委托
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
