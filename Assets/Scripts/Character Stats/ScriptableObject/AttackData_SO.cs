using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new AttackData_SO", menuName = "Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    [Header("普通攻击距离")]
    public float attackRange;
    [Header("技能攻击距离")]
    public float skillRange;
    [Header("攻击冷却时间")]
    public float coolDown;
    [Header("攻击最小伤害")]
    public int minDamage;
    [Header("攻击最大伤害")]
    public int maxDamage;

    // 暴击倍数 x 暴击率 - 后面会计算是否暴击
    [Header("暴击倍数")]
    public float criticalMultiplier;
    [Header("暴击概率")]
    public float criticalChance;
}
