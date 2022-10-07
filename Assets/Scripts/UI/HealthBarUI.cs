using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ע��, ��� Script ����ص�ÿ����Ҫ Ѫ���Ķ�����
/// 
/// �����Ҫ��ָ��������������, �������Ѫ��
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    // ע��, �� Unity-Editor ��, ֱ�ӽ� Bar Holder �ϵ� Script ��.
    // �������� OnEnable ʱ, ������� Canvas
    public GameObject healthUIPrefab;

    // Enemy ͷ��������λ��(��ʼ����ʱ���λ��).
    public Transform barPoint;

    // �Ƿ�һֱ�ɼ�
    public bool alwaysVisible;

    public float visiableTime;

    private float timeleft;
    // Ѫ���Ļ�����
    Image healthSlider;

    // Ϊ�����ɵ�ǰ�� HealthBar ʵ���Ժ�, ��UIBar ��ǰλ�����ó� barPoint λ��
    Transform UIbar;

    // ��Ҫ�����λ��, ��Ϊ��Ҫʵʱ,�� Bar �� Transform �� forward �������ó��������
    Transform cam;

    CharacterStats currentStats;

    // ��Ϊ��� HealthBar �ǹ�����ÿ�� Enemy ����, ���ÿ�� Enemy ���϶��� characterStats
    private void Awake()
    {
        currentStats = GetComponent<CharacterStats>();

        currentStats.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    private void OnEnable()
    {
        // ���ܳ��������ж�����
        cam = Camera.main.transform;

        // ������Enable��ʱ��, ���� UIBar... ������ʾ������ָ���� Canvas ��
        // ����, ��ǰ��������ӵ�ж�� Canvas
        foreach(Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                // ע��, Ѫ����Ҫ���� father canvas transform
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // ������ĵ�ǰ ����ֵ <= 0 ʱ, ��Ҫ���� UIbar ����
        if(currentHealth <= 0)
        {
            Destroy(UIbar.gameObject);
        }

        UIbar.gameObject.SetActive(true);
        timeleft = visiableTime;


        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    // �ƶ�Ѫ��. �����������, ҲӦ�������������Ⱦ����Ժ����
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
