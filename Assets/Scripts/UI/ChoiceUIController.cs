// Assets/Scripts/UI/ChoiceUIController.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the UI panel for presenting choices to the player.
/// </summary>
public class ChoiceUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject choicePanel;
    public Transform cardContainer;
    public GameObject optionCardPrefab;

    void Start()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Displays the upgrade choice UI with the given options.
    /// </summary>
    public void ShowUpgradeChoices(List<UpgradeData> choices)
    {
        // Clear any previous choice cards.
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // For each choice, instantiate and set up a new card.
        foreach (var choiceData in choices)
        {
            GameObject cardInstance = Instantiate(optionCardPrefab, cardContainer);
            OptionCardDisplay cardDisplay = cardInstance.GetComponent<OptionCardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(choiceData);
            }
        }

        choicePanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game while the player decides.
    }

    /// <summary>
    /// Hides the choice panel and resumes the game.
    /// </summary>
    public void HideChoicePanel()
    {
        choicePanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game.
    }
}