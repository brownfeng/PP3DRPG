using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    // 角色数据, 都放在 模型 SO 中
    public CharacterData_SO characterData;

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
}
