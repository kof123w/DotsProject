using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 普通单例工具
/// </summary>
public abstract class Singleton<T> where T : new (){
    private static T instance;
    private static Object mutex = new Object();

    public static T Instance {
        get {
            if (instance == null) {
                lock (mutex) {    //确保单列初始化是线程安全
                    if (instance == null) { 
                        instance = new T();
                    }
                }
            }

            return instance;
        }
    }
}

/// <summary>
/// unity的单列工具
/// </summary> 
public class UnitySingleton<T> : MonoBehaviour where T : Component {
    private static T instance;
    public static T Instance
    {
        get {
            if (instance==null) {
                instance =  FindObjectOfType(typeof(T)) as T;
                if (instance == null) {
                    GameObject go = new GameObject();
                    instance = (T)go.AddComponent(typeof(T));
                    go.hideFlags = HideFlags.DontSave;
                    go.name = typeof(T).Name;
                }
            }

            return (T)instance;
        }
    }

    public virtual void Awake() { 
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
        {
            instance = this as T;
        }
        else {
            Destroy(instance);
        }
    }
}