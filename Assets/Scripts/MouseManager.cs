using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventVector3 : UnityEvent<Vector3> { }
public class MouseManager : MonoBehaviour
{
    public EventVector3 OnMouseClick;

    private RaycastHit hitInfo;

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
    /// �ڰ�����������ʱ, �����������ָ�����ײ��
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
