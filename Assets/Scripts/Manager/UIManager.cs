using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : UnitySingleton<UIManager>
{
    [HideInInspector] public InputBarUI inputBarUI;
    [HideInInspector] public DialogueBubbleUI dialogueBubbleUI;

    public GameObject banUI;

    protected override void Awake()
    {
        base.Awake();

        GameObject canvas = GameObject.Find("OverlayCanvas");
        inputBarUI = canvas.transform.GetComponentInChildren<InputBarUI>();
        dialogueBubbleUI = canvas.transform.GetComponentInChildren<DialogueBubbleUI>();
    }

    private void Start()
    {
        AIManager.Instance.OnRequestBan += () => banUI.SetActive(true);
    }
}
