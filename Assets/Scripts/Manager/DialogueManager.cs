using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DialogueManager : UnitySingleton<DialogueManager>
{
    //记录的历史对话
    private LinkedList<Content> historyDialogue = new LinkedList<Content>();
  
    //最大Token
    public int MaxHistoryTokens = 1024;
    private int currentTokens = 0;

    //设定
    public string CharacterSetting = "";
    private Content CharacterSettingContent;

    //委托
    public event Action<Content> OnDialogueAdded;
    public event Action<Content> OnDialogueRemoved;
    public event Action OnClearHistory;

    protected override void Awake()
    {
        base.Awake();

        //初始化设定语句
        CharacterSettingContent = new Content()
        {
            role = "system",
            content = CharacterSetting
        };

        //更新最大Token
        MaxHistoryTokens -= CharacterSetting.Length;
    }

    private void Start()
    {
        //读取SaveManager，复原数据
        foreach(var content in SaveManager.Instance.data.dialogues)
        {
            AddHistory(content);
        }
    }

    //说话
    public void Talk(string contentText, Action<string> completeCallback, Action<string> streamingCallback)
    {
        //添加到历史记录
        AddHistory(Role.User, contentText);
        
        //用于发送到AIManager，拼接上玩家的角色设定
        List<Content> dialogue = new List<Content>() { CharacterSettingContent };
        dialogue.AddRange(historyDialogue);

        //日志打印
        StringBuilder sb = new StringBuilder();
        foreach(Content content in dialogue)
        {
            sb.Append(content.content).Append("\n");
        }
        Debug.LogWarning(sb.ToString());

        //发送给AI
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
        //确定当前加入历史记录的Role
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

        //添加到历史记录
        historyDialogue.AddLast(new Content() { role = roleStr, content = contentText });
        currentTokens += contentText.Length;

        //如果是用户发送的，就进行裁切
        if(role == Role.User)
        {
            //裁切历史文本
            while (currentTokens >= MaxHistoryTokens)
            {
                //发送删除委托
                OnDialogueRemoved?.Invoke(historyDialogue.First());

                currentTokens -= historyDialogue.First.Value.content.Length;
                historyDialogue.RemoveFirst();
            }
        }

        //序列化后保存到本地
        SaveManager.Instance.data.dialogues = historyDialogue;
        SaveManager.Instance.Save();

        StringBuilder sb = new StringBuilder();
        foreach (Content content in historyDialogue)
        {
            sb.Append(content.content).Append("\n");
        }
        Debug.Log(sb.ToString());

        //发送委托
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
