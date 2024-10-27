using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    //��ť��ҳ�������
    public Button openUI, closeUI, clearUI;
    public GameObject logPage;

    //�����õ�
    public ScrollRect scrollRect;
    public Transform entryParent;
    public GameObject aiEntryTemplate, userEntryTemplate;

    //������������
    public int MaxDisplayEntryNum = 20;
    public Queue<GameObject> entryQueue;

    //Э��ʹ�õ�
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
            //ѡ����һ���AIͷ��
            GameObject go;
            if (content.role == "user")
                go = Instantiate(userEntryTemplate);
            else
                go = Instantiate(aiEntryTemplate);

            //���ø��������ÿɼ���
            go.transform.SetParent(entryParent, false);
            go.SetActive(true);

            //������ʾ�ı�
            go.transform.GetComponentInChildren<TMP_Text>().text = content.content;

            //�����UI
            entryQueue.Enqueue(go);

            //������ʾ��Ŀ����
            while(entryQueue.Count > MaxDisplayEntryNum)
            {
                Destroy(entryQueue.Dequeue());
            }

            //�Զ�������ײ�
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

    // ����һ��Э��
    private IEnumerator ScrollToBottomAfterDelay()
    {
        // �ȴ�һ�룬��ȷ�� UI ��ȫ����
        yield return wait;

        // �Զ�������ײ�
        scrollRect.verticalNormalizedPosition = 0;
    }
}
