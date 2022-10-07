using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // ����ɫ���ݷ����ı�ʱ..
    public event Action<int, int> UpdateHealthBarOnAttack;

    // ���ɽ�ɫ���ݵ�ģ������.
    public CharacterData_SO templateData;

    // ��ɫ����, ������ ģ�� SO ��
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;// ��ǰ�Ƿ��ڱ���״̬

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

    // ����ʱ, �����˺� - �������� Animation Event �е���
    public void TakeDamage(CharacterStats attacker, CharacterStats defender)
    {
        // ��������ʱ, ��ǰ�Ĺ����ᵼ��
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence, 0);

        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

        // ��Ҫ���ݹ������Ƿ��Ǳ������ж�, �Ƿ�������ߴ���Hit����
        if (attacker.isCritical)
        {
            defender.GetComponent<Animator>().SetTrigger("Hit");
        }
        // TODO: Update UI

        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        // TODO: ���� Update
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
