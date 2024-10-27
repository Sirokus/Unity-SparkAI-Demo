using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    //按钮和页面的引用
    public Button openUI, closeUI, clearUI;
    public GameObject logPage;

    //生成用的
    public ScrollRect scrollRect;
    public Transform entryParent;
    public GameObject aiEntryTemplate, userEntryTemplate;

    //管理生命周期
    public int MaxDisplayEntryNum = 20;
    public Queue<GameObject> entryQueue;

    //协程使用的
    private WaitForSeconds wait = new WaitForSeconds(1);

    private void Awake()
    {
        entryQueue = new Queue<GameObject>(MaxDisplayEntryNum);
        
        openUI.onClick.AddListener(() =>
        {
            logPage.SetActive(true);
            AudioManager.Instance.PlayUIAudio(UIAudioType.ButtonPress);
        });
        closeUI.onClick.AddListener(() => logPage.SetActive(false));

        clearUI.onClick.AddListener(() =>
        {
            DialogueManager.Instance.ClearHistory();
        });
    }

    private void Start()
    {
        DialogueManager.Instance.OnDialogueAdded += (content) =>
        {
            //选择玩家或者AI头像
            GameObject go;
            if (content.role == "user")
                go = Instantiate(userEntryTemplate);
            else
                go = Instantiate(aiEntryTemplate);

            //设置父级，设置可见性
            go.transform.SetParent(entryParent, false);
            go.SetActive(true);

            //设置显示文本
            go.transform.GetComponentInChildren<TMP_Text>().text = content.content;

            //缓存该UI
            entryQueue.Enqueue(go);

            //限制显示条目数量
            while(entryQueue.Count > MaxDisplayEntryNum)
            {
                Destroy(entryQueue.Dequeue());
            }

            //自动滚到最底部
            StartCoroutine(ScrollToBottomAfterDelay());
        };

        DialogueManager.Instance.OnClearHistory += () =>
        {
            foreach (var go in entryQueue)
            {
                Destroy(go);
            }
            entryQueue.Clear();
        };
    }

    // 定义一个协程
    private IEnumerator ScrollToBottomAfterDelay()
    {
        // 等待一秒，以确保 UI 完全更新
        yield return wait;

        // 自动滚到最底部
        scrollRect.verticalNormalizedPosition = 0;
    }
}
