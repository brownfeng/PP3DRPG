using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName ="new CharacterData_SO", menuName = "Character Stats/Base Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]

    [Header("最大生命值")]
    public int maxHealth;

    [Header("当前生命值")]
    public int currentHealth;

    [Header("基础防御值")]
    public int baseDefence;

    [Header("当前防御值")]
    public int currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;

    public float levelMultiplier
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;
        if(currentExp >= baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // 所有想升级的数据的方法
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * levelMultiplier);

        maxHealth = (int)(maxHealth * levelMultiplier);
        currentHealth = maxHealth;

        Debug.Log("Level Up" + currentLevel + "Max Health:" + maxHealth);
    }

}
