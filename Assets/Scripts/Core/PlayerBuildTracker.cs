// Assets/Scripts/Core/PlayerBuildTracker.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System; // Required for Action

/// <summary>
/// Tracks the player's current set of towers, upgrades, and abilities for a run.
/// </summary>
public class PlayerBuildTracker : MonoBehaviour
{
    // Event to broadcast when a new upgrade is acquired.
    public event Action<UpgradeData> OnUpgradeAdded;

    // A list of all upgrades the player has acquired.
    public List<UpgradeData> ownedUpgrades = new List<UpgradeData>();

    /// <summary>
    /// Adds an upgrade to the player's build and notifies other systems.
    /// </summary>
    public void AddUpgrade(UpgradeData upgrade)
    {
        if (upgrade != null && !ownedUpgrades.Contains(upgrade))
        {
            ownedUpgrades.Add(upgrade);
            Debug.Log($"Player acquired upgrade: {upgrade.upgradeName}");

            // Broadcast that a new upgrade was added. The TowerManager will hear this.
            OnUpgradeAdded?.Invoke(upgrade);
        }
    }

    /// <summary>
    /// Checks if the player has a specific upgrade.
    /// </summary>
    public bool HasUpgrade(UpgradeData upgrade)
    {
        return ownedUpgrades.Contains(upgrade);
    }

    /// <summary>
    /// Checks if the player meets all prerequisites for a given upgrade.
    /// </summary>
    public bool MeetsPrerequisites(UpgradeData upgrade)
    {
        if (upgrade.prerequisites == null || upgrade.prerequisites.Count == 0)
        {
            return true;
        }
        return upgrade.prerequisites.All(prereq => ownedUpgrades.Contains(prereq));
    }

    /// <summary>
    /// Checks if the player has any upgrades that conflict with the given one.
    /// </summary>
    public bool HasExclusion(UpgradeData upgrade)
    {
        if (upgrade.exclusions == null || upgrade.exclusions.Count == 0)
        {
            return false;
        }
        return upgrade.exclusions.Any(exclusion => ownedUpgrades.Contains(exclusion));
    }
}