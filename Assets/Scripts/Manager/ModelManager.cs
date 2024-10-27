using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Thinking,
    LookAround1,
    LookAround2,
    HeadShake,
    Salute,
    Spin,
    Dance
}

public class ModelManager : UnitySingleton<ModelManager>
{
    public Animator modelAnimator;

    private float timer;

    private bool returnCenter = false;

    private void Start()
    {
        timer = Random.Range(15f, 30f);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            SetAction((ActionType)Random.Range(0, 7));
            timer = Random.Range(15f, 30f);
        }

        if(returnCenter)
        {
            if(Mathf.Approximately(Vector3.Dot(modelAnimator.transform.localPosition, Vector3.zero), 0) &&
                Mathf.Approximately(Quaternion.Angle(modelAnimator.transform.localRotation, Quaternion.identity), 0))
            {
                returnCenter = false;
            }

            modelAnimator.transform.localPosition = Vector3.Slerp(modelAnimator.transform.localPosition, Vector3.zero, 0.1f);
            modelAnimator.transform.localRotation = Quaternion.Slerp(modelAnimator.transform.localRotation, Quaternion.identity, 0.1f);
        }
    }

    public void SetAction(ActionType action)
    {
        string animatorParameter = System.Enum.GetName(typeof(ActionType), action);
        modelAnimator.SetTrigger(animatorParameter);
        StopReturnCenter();
    }

    public void ReturnCenter() => returnCenter = true;
    public void StopReturnCenter() => returnCenter = false;
}


