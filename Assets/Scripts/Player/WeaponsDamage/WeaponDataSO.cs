using UnityEngine;

public enum TargetType
{ 
    None,
    Rock,
    Tree,
    RockStructure,
    WoodStructure,
    Enemy,
    LooseObject,
    Indestructible
}

[CreateAssetMenu(menuName = "Inventory System/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Daño")]
    public int damage;

    [Header("Efectividad")]
    public TargetType[] effectiveWith;
}
