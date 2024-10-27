using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputBarUI : MonoBehaviour
{
    private TMP_InputField inputField;
    private Button sendBtn;

    private bool canSend = true;

    private void Awake()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        sendBtn = GetComponentInChildren<Button>();

        sendBtn.onClick.AddListener(() =>
        {
            //限制输入
            if (!canSend || inputField.text == "")
                return;
            canSend = false;

            //重置文本框
            string inputContent = inputField.text;
            inputField.text = "";

            //触发对话，管理对话气泡的更新
            UIManager.Instance.dialogueBubbleUI.ResetUI();
            DialogueManager.Instance.Talk(inputContent, (answer) =>
            {
                canSend = true;

                if(answer != null)
                    UIManager.Instance.dialogueBubbleUI.Play(answer);
            }, (stramingCallback) =>
            {
                UIManager.Instance.dialogueBubbleUI.Play(stramingCallback);
            });

            //让模型动一下
            ModelManager.Instance.SetAction(ActionType.Thinking);

            //播放音效
            AudioManager.Instance.PlayUIAudio(UIAudioType.SendBtn);
        }); 
    }
}
