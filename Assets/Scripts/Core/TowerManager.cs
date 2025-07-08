// Assets/Scripts/Core/TowerManager.cs
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    private PlayerBuildTracker playerBuildTracker;
    private List<Tower> activeTowers = new List<Tower>();

    void Awake()
    {
        playerBuildTracker = GetComponent<PlayerBuildTracker>();
        if (playerBuildTracker != null)
        {
            // Subscribe to the event. When an upgrade is added, HandleUpgradeAdded will be called.
            playerBuildTracker.OnUpgradeAdded += HandleUpgradeAdded;
        }
    }

    void OnDestroy()
    {
        if (playerBuildTracker != null)
        {
            // Always unsubscribe from events when the object is destroyed.
            playerBuildTracker.OnUpgradeAdded -= HandleUpgradeAdded;
        }
    }

    public void RegisterTower(Tower tower)
    {
        if (!activeTowers.Contains(tower))
        {
            activeTowers.Add(tower);

            // When a new tower is built, immediately apply all existing upgrades to it.
            foreach (var upgrade in playerBuildTracker.ownedUpgrades)
            {
                if (upgrade.upgradeType == UpgradeType.TowerModification)
                {
                    tower.ApplyModification(upgrade);
                }
            }
        }
    }

    public void UnregisterTower(Tower tower)
    {
        if (activeTowers.Contains(tower))
        {
            activeTowers.Remove(tower);
        }
    }

    /// <summary>
    /// This method is called whenever the PlayerBuildTracker gets a new upgrade.
    /// </summary>
    private void HandleUpgradeAdded(UpgradeData upgrade)
    {
        // If the upgrade is a tower modification, apply it to all active towers.
        if (upgrade.upgradeType == UpgradeType.TowerModification)
        {
            foreach (var tower in activeTowers)
            {
                tower.ApplyModification(upgrade);
            }
        }
        // If it's a new tower unlock, you could enable it in a build UI here.
        else if (upgrade.upgradeType == UpgradeType.TowerUnlock)
        {
            Debug.Log($"TOWER UNLOCKED: {upgrade.upgradeName}. Add logic to make it buildable.");
        }
    }
}