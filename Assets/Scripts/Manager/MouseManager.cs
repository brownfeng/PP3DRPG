using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

[System.Serializable]
public class MouseManager : Singleton<MouseManager>
{
    // ����һ�� UnityEvent �¼�, ���¼��ڷ���ʱ, �����һ�� Vector3 �ı���
    // ע��, �ڵ������UnityEventʱ, ����ʹ�� ?.Invoke(arg1) ��ʽ...
    // ����, ����EventVector3 ʹ�� [System.Serializable] ��������, �����UnityEditor��, �ᱩ¶���ûص�������ѡ��!!!
    public event Action<Vector3> OnMouseClick;

    public event Action<GameObject> OnAttackClick;

    public Texture2D Point, Doorway, Attack, Target, Arrow;

    private RaycastHit hitInfo;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    private void SetCursorTexture() {
        // 1. ʹ�� Camera.main ��ʾ, �ӵ�ǰScence������, ��ȡ Tag == "MainCamera" ��GameObject
        // 2. Input.mousePosition ���ص�ǰ�������Ļ�е���������!!!!
        // 3. ScreenPointToRay(v3) �� MainCamera �� ScreenPoint ����һ�� Ray ����
        // 4. ʹ�� Debug.DrawRay ��ִ��������!!! �� Scene �����ܿ���(Game���ڿ�����)
        // 5. Physics.RayCast �򳡾��е�������ײ�巢��һ������!!! ָ�����ߵ� origin, direction, maxDistance

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        // ����: ʹ���������ʱ, ���Ҫ�����ľ����ĵذ�, ��������ľ�ᵲס���λ�÷��͵�����, ��������ֽ������:
        // 1. ֱ���ڳ�����ѡ�����е���ľ, ѡ�����ǵ�LayerΪ 2 Ignore Rycast -- Ҳ���Ǻ������е�����
        // 2. �� Physics.Raycast, ���ǻ���������� collider �ĽӴ�, ���Խ������е���ľ�� MeshCollider disable.
        //  һ��ѡ�� 2, ʹ��NavMesh ���������, һ�㲻�����ľ��ײ��ģ (����: ����������,����ж���������, ��ô����ľ���������ײ)

        // ����: �����Ҫ�������, ��Ҫ�� Enemy �������� Box Collider, ������������޷�ѡ��, ����ͳһ����Tag 
        
        // ����: �����Ҫ����Ҳ��Ҫ�����ڵ��޳�, ��Ҫ�� URP Render ������!!!
        if (Physics.Raycast(ray, out hitInfo)) { 
            /// �л������ͼ
            switch(hitInfo.collider.tag)
            {
                case "Ground":
                    Cursor.SetCursor(Target, new Vector2(16, 16), CursorMode.Auto);
                  break;
                case "Enemy":
                    Cursor.SetCursor(Attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    /// <summary>
    /// �ڰ�����������ʱ, �����������ָ�����ײ��Ƚ� Tag, Ȼ�󴥷������������ص�����
    /// </summary>
    private void MouseControl() { 
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            // GetMouseButton(0) �����Ƿ����˸�������갴ť��
            // hitInfo ��һ�� RaycastHit ��Ϣ, ����������ǰ�����е�����
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                // ע��, ����ʱ, ����ʹ�� ?.Invoke(arg1) ��ʽ...
                OnMouseClick?.Invoke(hitInfo.point);
            }

            if(hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                OnAttackClick?.Invoke(hitInfo.collider.gameObject);
            }
        }
    }
}
