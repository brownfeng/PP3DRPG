using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance;
    public event Action<Vector3> OnMouseClick;

    public Texture2D Point, Doorway, Attack, Target, Arrow;

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
        /// 核心过程: 使用鼠标 mousePosition 发送一个射线, 根据射线碰撞的对象, 设置鼠标的贴图.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo)) { 
            /// 切换鼠标贴图
            switch(hitInfo.collider.tag)
            {
                case "Ground":
                    Cursor.SetCursor(Target, new Vector2(16, 16), CursorMode.Auto);
                  break;
            }
        }
    }

    /// <summary>
    /// 在按下鼠标左键的时, 根据鼠标射线指向的碰撞体比较 Tag, 然后触发对外的鼠标点击回调方法
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
