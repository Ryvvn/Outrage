// Assets/Scripts/Data/UpgradeData.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enum to define the rarity of an upgrade. Affects drop chance and visual representation.
/// </summary>
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

/// <summary>
/// Enum to categorize the type of upgrade.
/// </summary>
public enum UpgradeType
{
    TowerUnlock,
    TowerModification,
    GlobalAbility
}

/// <summary>
/// A serializable class to hold information about a single stat modification.
/// </summary>
[System.Serializable]
public class StatModification
{
    public string statName; // e.g., "Damage", "Range"
    public float value;
    public bool isPercentage;
}

/// <summary>
/// ScriptableObject to hold all data for a single upgrade.
/// This can be a tower, a tower modification, or a global ability.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Tower Defense/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Core Info")]
    public string upgradeID; // Unique identifier, e.g., "TOWER_ARCHER_DAMAGE_1"
    public string upgradeName;
    [TextArea(3, 10)]
    public string description;
    public Sprite icon;
    public Rarity rarity;
    public UpgradeType upgradeType;

    [Header("Gameplay Effects")]
    // List of direct stat changes this upgrade provides.
    public List<StatModification> statModifications;
    // Special effect identifier, e.g., "Poison", "ChainLightning"
    public string specialEffect;

    [Header("Conditions")]
    public int unlockWave; // Minimum wave number for this upgrade to appear.
    // Upgrades required before this one can be offered.
    public List<UpgradeData> prerequisites;
    // Upgrades that cannot be active at the same time as this one.
    public List<UpgradeData> exclusions;
}