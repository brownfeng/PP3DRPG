using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new AttackData_SO", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    [Header("��ͨ��������")]
    public float attackRange;
    [Header("���ܹ�������")]
    public float skillRange;
    [Header("������ȴʱ��")]
    public float coolDown;
    [Header("������С�˺�")]
    public int minDamage;
    [Header("��������˺�")]
    public int maxDamage;

    // �������� x ������ - ���������Ƿ񱩻�
    [Header("��������")]
    public float criticalMultiplier;
    [Header("��������")]
    public float criticalChance;
}
