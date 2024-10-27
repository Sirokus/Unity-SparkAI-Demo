using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GameManager : UnitySingleton<GameManager>
{
    public Light directionalLight;

    private void Start()
    {
        SetLightAngleHorizontal(SaveManager.Instance.data.lightHorizontalAngle);
        SetLightAngleVertical(SaveManager.Instance.data.lightVerticalAngle);
    }

    public void SetLightAngleHorizontal(float horizontalAngle)
    {
        Vector3 eulerAngles = directionalLight.transform.rotation.eulerAngles;
        eulerAngles.y = horizontalAngle;
        directionalLight.transform.rotation = Quaternion.Euler(eulerAngles);
    }

    public void SetLightAngleVertical(float verticalAngle)
    {
        float yAngle = directionalLight.transform.rotation.eulerAngles.y;
        directionalLight.transform.rotation = Quaternion.Euler(verticalAngle, 0, 0);
        SetLightAngleHorizontal(yAngle);
    }
}
