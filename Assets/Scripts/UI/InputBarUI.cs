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
            //��������
            if (!canSend || inputField.text == "")
                return;
            canSend = false;

            //�����ı���
            string inputContent = inputField.text;
            inputField.text = "";

            //�����Ի�������Ի����ݵĸ���
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

            //��ģ�Ͷ�һ��
            ModelManager.Instance.SetAction(ActionType.Thinking);

            //������Ч
            AudioManager.Instance.PlayUIAudio(UIAudioType.SendBtn);
        }); 
    }
}
