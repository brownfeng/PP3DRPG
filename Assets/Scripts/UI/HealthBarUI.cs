using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 注意, 这个 Script 会挂载到每个需要 血条的对象上
/// 
/// 因此需要在指定的生命周期中, 创建这个血条
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    // 注意, 在 Unity-Editor 中, 直接将 Bar Holder 拖到 Script 中.
    // 可能是在 OnEnable 时, 创建这个 Canvas
    public GameObject healthUIPrefab;

    // Enemy 头顶的坐标位置(初始化的时候的位置).
    public Transform barPoint;

    // 是否一直可见
    public bool alwaysVisible;

    public float visiableTime;

    private float timeleft;
    // 血条的滑动条
    Image healthSlider;

    // 为了生成当前的 HealthBar 实体以后, 将UIBar 当前位置设置成 barPoint 位置
    Transform UIbar;

    // 需要摄像机位置, 因为需要实时,将 Bar 的 Transform 的 forward 方向设置成面向相机
    Transform cam;

    CharacterStats currentStats;

    // 因为这个 HealthBar 是挂载在每个 Enemy 身上, 因此每个 Enemy 身上都有 characterStats
    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        // 可能场景里面有多个相机
        cam = Camera.main.transform;

        // 另外在Enable的时候, 创建 UIBar... 并且显示到我们指定的 Canvas 上
        // 但是, 当前场景可能拥有多个 Canvas
        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                // 注意, 血条需要设置 father canvas transform
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // 当怪物的当前 生命值 <= 0 时, 需要销毁 UIbar 对象
        if(currentHealth <= 0)
        {
            Destroy(UIbar.gameObject);
        }

        UIbar.gameObject.SetActive(true);
        timeleft = visiableTime;


        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    // 移动血条. 类似于摄像机, 也应该在摄像机在渲染完成以后更新
    private void LateUpdate()
    {
        if(UIbar != null)
        {
            UIbar.position = barPoint.position;
            UIbar.forward = -cam.forward;

            if(timeleft <=0 && !alwaysVisible)
            {
                UIbar.gameObject.SetActive(false);
            } else
            {
                timeleft -= Time.deltaTime;
            }
        }
    }
}
