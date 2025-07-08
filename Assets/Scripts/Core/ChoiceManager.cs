// Assets/Scripts/Core/ChoiceManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages choice events, generating upgrade options and interacting with the UI.
/// </summary>
public class ChoiceManager : MonoBehaviour
{
    [Header("Dependencies")]
    public ChoiceUIController choiceUIController;
    public PlayerBuildTracker playerBuildTracker;
    public WaveManager waveManager; // To get the current wave number

    [Header("Upgrade Pool")]
    // The master list of all possible upgrades in the game.
    public List<UpgradeData> masterUpgradePool = new List<UpgradeData>();

    [Header("Choice Parameters")]
    public int numberOfChoices = 3;
    public int wavesPerUpgradeChoice = 1;

    // Rarity weights for upgrade selection, matching the requirements.
    private readonly Dictionary<Rarity, float> rarityWeights = new Dictionary<Rarity, float>
    {
        { Rarity.Common, 0.60f },
        { Rarity.Uncommon, 0.25f },
        { Rarity.Rare, 0.12f },
        { Rarity.Legendary, 0.03f }
    };

    void Start()
    {
        if (waveManager != null)
        {
            // Subscribe to the wave completion event to trigger choices.
            waveManager.OnWaveCompleted += HandleWaveCompleted;
        }
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveCompleted -= HandleWaveCompleted;
        }
    }

    /// <summary>
    /// Called by the WaveManager when a wave is completed.
    /// </summary>
    private void HandleWaveCompleted(int completedWaveNumber)
    {
        if (completedWaveNumber > 0 && completedWaveNumber % wavesPerUpgradeChoice == 0)
        {
            TriggerUpgradeChoice();
        }
    }

    /// <summary>
    /// Starts the process of showing upgrade choices to the player.
    /// </summary>
    public void TriggerUpgradeChoice()
    {
        List<UpgradeData> choices = GenerateUpgradeChoices();
        if (choices.Count > 0)
        {
            choiceUIController.ShowUpgradeChoices(choices);
        }
        else
        {
            Debug.LogWarning("No valid upgrade choices could be generated.");
        }
    }

    /// <summary>
    /// Generates a list of random upgrade choices for the player.
    /// </summary>
    private List<UpgradeData> GenerateUpgradeChoices()
    {
        int currentWave = waveManager.currentWaveIndex + 1;

        // Filter the master pool to get only currently valid upgrades.
        List<UpgradeData> validUpgrades = masterUpgradePool.Where(u =>
            !playerBuildTracker.HasUpgrade(u) &&
            currentWave >= u.unlockWave &&
            playerBuildTracker.MeetsPrerequisites(u) &&
            !playerBuildTracker.HasExclusion(u)
        ).ToList();

        var choices = new List<UpgradeData>();
        var tempPool = new List<UpgradeData>(validUpgrades);

        for (int i = 0; i < numberOfChoices && tempPool.Count > 0; i++)
        {
            UpgradeData choice = SelectUpgradeByRarity(tempPool);
            if (choice != null)
            {
                choices.Add(choice);
                tempPool.Remove(choice); // Ensure it's not picked again in the same batch.
            }
        }

        return choices;
    }

    /// <summary>
    /// Selects a single upgrade from a pool based on rarity weights.
    /// </summary>
    private UpgradeData SelectUpgradeByRarity(List<UpgradeData> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        var presentRarities = pool.Select(u => u.rarity).Distinct();
        float totalWeight = presentRarities.Sum(r => rarityWeights[r]);
        float randomValue = Random.Range(0, totalWeight);

        float cumulativeWeight = 0;
        foreach (Rarity r in presentRarities.OrderBy(r => r)) // Consistent order
        {
            cumulativeWeight += rarityWeights[r];
            if (randomValue <= cumulativeWeight)
            {
                var upgradesOfRarity = pool.Where(u => u.rarity == r).ToList();
                return upgradesOfRarity[Random.Range(0, upgradesOfRarity.Count)];
            }
        }

        return pool[Random.Range(0, pool.Count)]; // Fallback
    }

    /// <summary>
    /// Called from a choice card when the player makes a selection.
    /// </summary>
    public void OnUpgradeSelected(UpgradeData chosenUpgrade)
    {
        playerBuildTracker.AddUpgrade(chosenUpgrade);
        choiceUIController.HideChoicePanel();
    }
}