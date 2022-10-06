using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    public static T Instance
    {
        get { return instance; }
    }

    /// <summary>
    /// 一个游戏物体挂载的脚本中Awake、Start只会执行一次，
    /// 当这个游戏物体被取消激活 再重新激活的时候，
    /// 脚本中的Awake、Start都不会再重新执行。而OnEnable会重新在第一帧执行一次！
    /// 
    /// 执行顺序：Awake -> OnEnable-> Start
    /// </summary>
    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = (T)this;
        }
    }

    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    protected virtual void OnDestroy()
    {
        if(instance == this)
        {
            instance = null;
        }
    }
}
