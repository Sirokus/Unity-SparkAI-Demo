using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DialogueManager : UnitySingleton<DialogueManager>
{
    //��¼����ʷ�Ի�
    private LinkedList<Content> historyDialogue = new LinkedList<Content>();
  
    //���Token
    public int MaxHistoryTokens = 1024;
    private int currentTokens = 0;

    //�趨
    public string CharacterSetting = "";
    private Content CharacterSettingContent;

    //ί��
    public event Action<Content> OnDialogueAdded;
    public event Action<Content> OnDialogueRemoved;
    public event Action OnClearHistory;

    protected override void Awake()
    {
        base.Awake();

        //��ʼ���趨���
        CharacterSettingContent = new Content()
        {
            role = "system",
            content = CharacterSetting
        };

        //�������Token
        MaxHistoryTokens -= CharacterSetting.Length;
    }

    private void Start()
    {
        //��ȡSaveManager����ԭ����
        foreach(var content in SaveManager.Instance.data.dialogues)
        {
            AddHistory(content);
        }
    }

    //˵��
    public void Talk(string contentText, Action<string> completeCallback, Action<string> streamingCallback)
    {
        //��ӵ���ʷ��¼
        AddHistory(Role.User, contentText);
        
        //���ڷ��͵�AIManager��ƴ������ҵĽ�ɫ�趨
        List<Content> dialogue = new List<Content>() { CharacterSettingContent };
        dialogue.AddRange(historyDialogue);

        //��־��ӡ
        StringBuilder sb = new StringBuilder();
        foreach(Content content in dialogue)
        {
            sb.Append(content.content).Append("\n");
        }
        Debug.LogWarning(sb.ToString());

        //���͸�AI
        AIManager.Instance.RequestAnswer(dialogue, (answer) =>
        {
            if(answer != null)
                AddHistory(Role.AI, answer);

            completeCallback(answer);
        }, streamingCallback);
    }

    public void AddHistory(Content content)
    {
        Role role = Role.User;
        switch(content.role)
        {
        case "system":
            role = Role.System;
            break;
        case "user":
            role = Role.User;
            break;
        case "assistant":
            role = Role.AI;
            break;
        }

        AddHistory(role, content.content);
    }

    public void AddHistory(Role role, string contentText)
    {
        //ȷ����ǰ������ʷ��¼��Role
        string roleStr = "";
        switch(role)
        {
        case Role.System:
            roleStr = "system";
            break;
        case Role.User:
            roleStr = "user";
            break;
        case Role.AI:
            roleStr = "assistant";
            break;
        default:
            break;
        }

        //��ӵ���ʷ��¼
        historyDialogue.AddLast(new Content() { role = roleStr, content = contentText });
        currentTokens += contentText.Length;

        //������û����͵ģ��ͽ��в���
        if(role == Role.User)
        {
            //������ʷ�ı�
            while (currentTokens >= MaxHistoryTokens)
            {
                //����ɾ��ί��
                OnDialogueRemoved?.Invoke(historyDialogue.First());

                currentTokens -= historyDialogue.First.Value.content.Length;
                historyDialogue.RemoveFirst();
            }
        }

        //���л��󱣴浽����
        SaveManager.Instance.data.dialogues = historyDialogue;
        SaveManager.Instance.Save();

        StringBuilder sb = new StringBuilder();
        foreach (Content content in historyDialogue)
        {
            sb.Append(content.content).Append("\n");
        }
        Debug.Log(sb.ToString());

        //����ί��
        OnDialogueAdded?.Invoke(historyDialogue.Last());
    }

    public void ClearHistory()
    {
        historyDialogue.Clear();
        currentTokens = 0;
        SaveManager.Instance.data.dialogues.Clear();
        SaveManager.Instance.Save();

        OnClearHistory?.Invoke();
    }
}
