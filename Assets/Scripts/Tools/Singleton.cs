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
    /// һ����Ϸ������صĽű���Awake��Startֻ��ִ��һ�Σ�
    /// �������Ϸ���屻ȡ������ �����¼����ʱ��
    /// �ű��е�Awake��Start������������ִ�С���OnEnable�������ڵ�һִ֡��һ�Σ�
    /// 
    /// ִ��˳��Awake -> OnEnable-> Start
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
