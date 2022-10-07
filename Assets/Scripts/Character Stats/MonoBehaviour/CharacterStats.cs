using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // 当角色数据发生改变时..
    public event Action<int, int> UpdateHealthBarOnAttack;

    // 生成角色数据的模板数据.
    public CharacterData_SO templateData;

    // 角色数据, 都放在 模型 SO 中
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;// 当前是否处于暴击状态

    private void Awake()
    {
        if (templateData != null)
        {
            characterData = Instantiate(templateData);
        }
    }

    #region Read from Data_SO
    public int MaxHealth {
        get
        {
            if (characterData == null)
            {
                return 0;
            }
            else return characterData.maxHealth;
        }
        set { characterData.maxHealth = value; }
    }

    public int CurrentHealth
    {
        get
        {
            if (characterData == null)
            {
                return 0;
            }
            else return characterData.currentHealth;
        }
        set { characterData.currentHealth = value; }
    }

    public int BaseDefence
    {
        get
        {
            if (characterData == null)
            {
                return 0;
            }
            else return characterData.baseDefence;
        }
        set { characterData.baseDefence = value; }
    }

    public int CurrentDefence
    {
        get
        {
            if (characterData == null)
            {
                return 0;
            }
            else return characterData.currentDefence;
        }
        set { characterData.currentDefence = value; }
    }

    #endregion

    #region Read from Attack_SO
    public float AttackRange
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.attackRange;
        }
        set { attackData.attackRange = value; }
    }

    public float SkillRange
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.skillRange;
        }
        set { attackData.skillRange = value; }
    }

    public float CoolDown
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.coolDown;
        }
        set { attackData.coolDown = value; }
    }

    public int MinDamage
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.minDamage;
        }
        set { attackData.minDamage = value; }
    }

    public int MaxDamage
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.maxDamage;
        }
        set { attackData.maxDamage = value; }
    }
    public float CriticalMultiplier
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.criticalMultiplier;
        }
        set { attackData.criticalMultiplier = value; }
    }

    public float CriticalChance
    {
        get
        {
            if (attackData == null)
            {
                return 0;
            }
            else return attackData.criticalChance;
        }
        set { attackData.criticalChance = value; }
    }
    #endregion

    #region Character Combat

    // 攻击时, 产生伤害 - 方法会在 Animation Event 中调用
    public void TakeDamage(CharacterStats attacker, CharacterStats defender)
    {
        // 发生攻击时, 当前的攻击会导致
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence, 0);

        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        // 需要根据攻击者是否是暴击来判断, 是否给防御者触发Hit动画
        if (attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        // TODO: Update UI

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        // TODO: 经验 Update
    }

    public void TakeDamage(int damage, CharacterStats defender)
    {
        int currentDamage = Mathf.Max(damage - defender.CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - currentDamage, 0);

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }
        return (int)coreDamage;
    }
    #endregion
}
