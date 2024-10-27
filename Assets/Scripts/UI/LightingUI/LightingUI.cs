using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightingUI : MonoBehaviour
{
    public Button lightingOpenCloseUI;
    public GameObject lightingPage;

    private void Awake()
    {
        lightingOpenCloseUI.onClick.AddListener(() =>
        {
            lightingPage.SetActive(!lightingPage.activeSelf);
            AudioManager.Instance.PlayUIAudio(UIAudioType.ButtonPress);
        });
    }

    private void Start()
    {
        lightingPage.SetActive(false);
    }
}
