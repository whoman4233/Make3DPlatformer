using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public enum ItemType
{
    Equipable,
    Consumable,
    Resource
}

public enum ConsumableType
{
    Health,
    SpeedBoost,
    JumpBoost
}

[Serializable]
public class ItemDataCunsumable
{
    public ConsumableType type; 
    public float value;// Health/Hunger는 양, SpeedBoost는 배수(예: 1.5f)
    public float duration = 5f;
}

[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string displayName;
    public string description;
    public ItemType type;
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;

    [Header("Consumable")]
    public ItemDataCunsumable[] consumables;

    [Header("Equip")]
    public GameObject equipPrefab;
}
