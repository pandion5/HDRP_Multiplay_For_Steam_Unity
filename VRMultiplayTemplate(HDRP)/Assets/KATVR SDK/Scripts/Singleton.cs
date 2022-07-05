using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static bool searchForInstance=true;

    public static T Instance
    {
        get
        {
            if(instance==null&&searchForInstance)
            {
                searchForInstance = false;
                T[] objects = GameObject.FindObjectsOfType<T>();
                if(objects.Length==1)
                {
                    instance = objects[0];

                }
                else if(objects.Length>1)
                {
                    Debug.LogErrorFormat("Too many instances had created.");
                }
            }
            return instance;
        }

    }
    public static bool IsInitialized
    {
        get
        {
            return instance != null;
        }
    }
    protected virtual void Awake()
    {
        if(IsInitialized&&instance!=this)
        {
            if(Application.isEditor)
            {
                DestroyImmediate(this);
            }
            else
            {
                Destroy(this);
            }
        }else if(!IsInitialized)
        {
            instance = (T)this;
        }
    }
    
    protected virtual void OnDestroy()
    {
        if(instance==this)
        {
            instance = null;
            searchForInstance = true;
        }
    }
}
