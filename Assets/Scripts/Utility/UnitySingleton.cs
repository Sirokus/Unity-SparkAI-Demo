using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance { get => _instance; }

    protected virtual void Awake()
    {
        if(_instance)
        {
            Destroy(this);
            return;
        }

        _instance = this as T;
    }
}
