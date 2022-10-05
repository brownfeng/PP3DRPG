using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName ="new CharacterData_SO", menuName = "Character Stats/Base Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]

    [Header("�������ֵ")]
    public int maxHealth;

    [Header("��ǰ����ֵ")]
    public int currentHealth;

    [Header("��������ֵ")]
    public int baseDefence;

    [Header("��ǰ����ֵ")]
    public int currentDefence;

}
