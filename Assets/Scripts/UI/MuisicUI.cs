using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MuisicUI : MonoBehaviour
{
    //操作按钮
    public Button OpenCloseUI;
    public Button PlayUI, PauseUI, LastSongUI, NextSongUI;

    //面板
    private RectTransform muisicPage;

    //歌名歌手
    public TMP_Text songTitle, singer;

    private bool isShow = false;

    private void Awake()
    {
        //面板
        muisicPage = OpenCloseUI.transform.parent.GetComponent<RectTransform>();

        //打开，关闭面板
        OpenCloseUI.onClick.AddListener(() =>
        {
            muisicPage.DOAnchorPosX(isShow ? 151 : -5, .5f);
            isShow = !isShow;
            OpenCloseUI.GetComponentInChildren<TMP_Text>().text = isShow ? ">" : "<";

            AudioManager.Instance.PlayUIAudio(UIAudioType.ButtonPress);
        });

        //播放
        PlayUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayBGM();
        });

        //暂停
        PauseUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.PauseBGM();
        });

        //上一首
        LastSongUI.onClick.AddListener(() =>
        {
            AudioManager.Instance.SelectLastBGM();
        });

        //下一首
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
