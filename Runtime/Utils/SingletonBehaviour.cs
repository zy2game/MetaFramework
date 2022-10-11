using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T mSingleton;
    public static T Instance
    {
        get
        {
            if (!mSingleton)
            {
                GameObject obj = GameObject.Find("Singleton");
                if (!obj)
                {
                    obj = new GameObject("Singleton");
                    DontDestroyOnLoad(obj);
                }
                mSingleton = obj.GetComponent<T>();
                if (!mSingleton)
                    mSingleton = obj.AddComponent<T>();
            }
            return mSingleton;
        }
    }
}
