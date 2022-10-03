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
        /// 核心过程: 使用鼠标 mousePosition 发送一个射线, 根据射线碰撞的对象, 设置鼠标的贴图.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo)) { 
            /// 切换鼠标贴图
        }
    }

    /// <summary>
    /// 在按下鼠标左键的时, 根据鼠标射线指向的碰撞体
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
