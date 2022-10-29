using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

[System.Serializable]
public class MouseManager : Singleton<MouseManager>
{
    // 声明一个 UnityEvent 事件, 该事件在发生时, 会接受一个 Vector3 的变量
    // 注意, 在调用这个UnityEvent时, 可以使用 ?.Invoke(arg1) 方式...
    // 另外, 由于EventVector3 使用 [System.Serializable] 进行声明, 因此在UnityEditor中, 会暴露配置回调函数的选项!!!
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

    private void SetCursorTexture()
    {
        // 1. 使用 Camera.main 表示, 从当前Scence场景中, 获取 Tag == "MainCamera" 的GameObject
        // 2. Input.mousePosition 返回当前鼠标在屏幕中的像素坐标!!!!
        // 3. ScreenPointToRay(v3) 从 MainCamera 到 ScreenPoint 发射一条 Ray 射线
        // 4. 使用 Debug.DrawRay 回执这条射线!!! 在 Scene 窗口能看到(Game窗口看不到)
        // 5. Physics.RayCast 向场景中的所有碰撞体发送一条射线!!! 指定射线的 origin, direction, maxDistance

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction, Color.red);

        // 问题: 使用鼠标射线时, 如果要点击树木后面的地板, 则由于树木会挡住鼠标位置发送的射线, 因此有两种解决方法:
        // 1. 直接在场景中选择所有的树木, 选择它们的Layer为 2 Ignore Rycast -- 也就是忽略所有的射线
        // 2. 在 Physics.Raycast, 我们会根据射线与 collider 的接触, 可以将场景中的树木的 MeshCollider disable.
        //  一般选用 2, 使用NavMesh 导航情况下, 一般不会和树木碰撞穿模 (优势: 后续开发中,如果有东西爆出来, 那么和树木不会产生碰撞)

        // 问题: 如果需要点击敌人, 需要在 Enemy 身上增加 Box Collider, 否则鼠标射线无法选中, 并且统一增加Tag 

        // 问题: 如果需要敌人也需要启动遮挡剔除, 需要在 URP Render 中配置!!!
        if (Physics.Raycast(ray, out hitInfo))
        {
            /// 切换鼠标贴图
            switch (hitInfo.collider.tag)
            {
                case "Ground":
                    Cursor.SetCursor(Target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(Attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(Doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(Arrow, new Vector2(16, 16), CursorMode.Auto);

                    break;
            }
        }
    }

    /// <summary>
    /// 在按下鼠标左键的时, 根据鼠标射线指向的碰撞体比较 Tag, 然后触发对外的鼠标点击回调方法
    /// </summary>
    private void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            // GetMouseButton(0) 返回是否按下了给定的鼠标按钮。
            // hitInfo 是一个 RaycastHit 信息, 用来描述当前服务中的内容
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                // 注意, 调用时, 可以使用 ?.Invoke(arg1) 方式...
                OnMouseClick?.Invoke(hitInfo.point);
            }

            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                OnAttackClick?.Invoke(hitInfo.collider.gameObject);
            }

            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                OnAttackClick?.Invoke(hitInfo.collider.gameObject);
            }

            if (hitInfo.collider.gameObject.CompareTag("Portal"))
            {
                OnMouseClick?.Invoke(hitInfo.point);
            }

        }
    }
}
