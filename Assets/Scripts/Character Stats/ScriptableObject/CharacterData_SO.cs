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

}
