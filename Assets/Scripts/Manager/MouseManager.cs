using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;
    public event Action<Vector3> OnMouseClick;

    private RaycastHit hitInfo;
    private void Awake()
    {
        if (MouseManager.Instance != null)
        {
            Destroy(this);
        }
        else {
            MouseManager.Instance = this;
        }
    }
    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    private void SetCursorTexture() {
        /// ���Ĺ���: ʹ����� mousePosition ����һ������, ����������ײ�Ķ���, ����������ͼ.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo)) { 
            /// �л������ͼ
        }
    }

    /// <summary>
    /// �ڰ�����������ʱ, �����������ָ�����ײ��Ƚ� Tag, Ȼ�󴥷������������ص�����
    /// </summary>
    private void MouseControl() { 
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if(hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                OnMouseClick.Invoke(hitInfo.point);
            }
        }
    }
}
