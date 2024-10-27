using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MuisicUI : MonoBehaviour
{
    //������ť
    public Button OpenCloseUI;
    public Button PlayUI, PauseUI, LastSongUI, NextSongUI;

    //���
    private RectTransform muisicPage;

    //��������
    public TMP_Text songTitle, singer;

    private bool isShow = false;

    private void Awake()
    {
        //���
        muisicPage = OpenCloseUI.transform.parent.GetComponent<RectTransform>();

        //�򿪣��ر����
        OpenCloseUI.onClick.AddListener(() =>
        {
            muisicPage.DOAnchorPosX(isShow ? 151 : -5, .5f);
            isShow = !isShow;
            OpenCloseUI.GetComponentInChildren<TMP_Text>().text = isShow ? ">" : "<";

            AudioManager.Instance.PlayUIAudio(UIAudioType.ButtonPress);
        });

        //����
        PlayUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBGM();
        });

        //��ͣ
        PauseUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.PauseBGM();
        });

        //��һ��
        LastSongUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.SelectLastBGM();
        });

        //��һ��
        NextSongUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.SelectNextBGM();
        });
    }

    private void Start()
    {
        AudioManager.Instance.OnBGMSelected += (bgmAudioClipConfig) =>
        {
            songTitle.text = bgmAudioClipConfig.SongTitle;
            singer.text = bgmAudioClipConfig.Singer;
        };

        AudioManager.Instance.OnBGMPlay += () =>
        {
            PlayUI.gameObject.SetActive(false);
            PauseUI.gameObject.SetActive(true);
        };

        AudioManager.Instance.OnBGMPause += () =>
        {
            PlayUI.gameObject.SetActive(true);
            PauseUI.gameObject.SetActive(false);
        };
    }
}
