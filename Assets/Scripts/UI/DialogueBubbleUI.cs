using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBubbleUI : MonoBehaviour
{
    //UI引用
    public TMP_Text shortTextUI, longTextUI;
    public GameObject shortUI, longUI;
    private ScrollRect longUIScrollRect;

    //分割属性
    public int splitLength = 360;
    public int limitShortTextUIHeigt = 171;

    //打字机效果
    TweenerCore<string, string, StringOptions> tweener;
    public float typeSpeed = .01f;

    //临时存储
    private string lastTextContent = "";

    private void Awake()
    {
        longUIScrollRect = longUI.GetComponent<ScrollRect>();
    }

    //隐藏和复位所有对话框
    public void ResetUI()
    {
        shortUI.SetActive(false);
        shortTextUI.text = string.Empty;
        longUI.SetActive(false);
        longTextUI.text = string.Empty;
    }

    public void PlayFromStart(string endText)
    {
        ResetUI();

        Play(endText);
    }

    public void Play(string endText)
    {
        string startText;
        if (longTextUI.text != string.Empty)
            startText = longTextUI.text;
        else
            startText = shortTextUI.text;

        //DoTween实现打字机效果
        tweener?.Kill();
        tweener = DOTween.To(() => startText, value =>
        {
            //中间判断说的话的长度，确定是否需要转到大的对话框（可下拉的那种）
            //达到阈值后转为显示大对话框
            if (shortTextUI.rectTransform.rect.height > limitShortTextUIHeigt && shortTextUI.text.Length > 0)
            {
                if (!longUI.activeSelf)
                {
                    shortUI.SetActive(false);
                    longUI.SetActive(true);
                }

                longTextUI.text = value;
                longUIScrollRect.verticalNormalizedPosition = 0f;
            }
            else
            {
                if (!shortUI.activeSelf)
                {
                    shortUI.SetActive(true);
                    longUI.SetActive(false);
                }

                shortTextUI.text = value;
            }

            //播放UI音效
            if (lastTextContent != value)
                AudioManager.Instance.PlayUIAudio(UIAudioType.DialoguePopText);
            lastTextContent = value;

        }, endText, endText.Length * typeSpeed).SetUpdate(true).SetEase(Ease.Linear);
    }
}
