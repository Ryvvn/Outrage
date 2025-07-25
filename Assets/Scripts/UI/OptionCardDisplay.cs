// Assets/Scripts/UI/OptionCardDisplay.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the display of a single choice card in the UI.
/// </summary>
public class OptionCardDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image previewImage;
    public TextMeshProUGUI rarityText;
    public Image borderImage; // To color-code rarity

    private UpgradeData associatedUpgrade;
    //private ChoiceManager choiceManager;

    // Defines the colors for each rarity tier.
    private readonly Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color>
    {
        { Rarity.Common, Color.gray },
        { Rarity.Uncommon, new Color(0.1f, 0.8f, 0.1f) }, // Green
        { Rarity.Rare, new Color(0.2f, 0.6f, 1f) },     // Blue
        { Rarity.Legendary, new Color(1f, 0.7f, 0f) }      // Orange
    };

    void Start()
    {
        //choiceManager = FindObjectOfType<ChoiceManager>();
    }

    /// <summary>
    /// Sets up the card with data from an UpgradeData object.
    /// </summary>
    public void Setup(UpgradeData data)
    {
        associatedUpgrade = data;

        titleText.text = data.upgradeName;
        descriptionText.text = data.description;
        previewImage.sprite = data.icon;

        // Handle rarity display.
        if (rarityText != null)
        {
            rarityText.text = data.rarity.ToString();
            if (rarityColors.TryGetValue(data.rarity, out Color color))
            {
                rarityText.color = color;
                if (borderImage != null)
                {
                    borderImage.color = color;
                }
            }
        }
    }

    /// <summary>
    /// Handles the click event for this card, notifying the ChoiceManager.
    /// </summary>
    public void HandleClick()
    {
        //if (associatedUpgrade != null && choiceManager != null)
        //{
        //    choiceManager.OnUpgradeSelected(associatedUpgrade);
        //}
    }

    // These methods are stubs for the hover effects defined in the prefab's EventTrigger.
    public void OnHoverEnter() { /* Future logic for tooltips can go here. */ }
    public void OnHoverExit() { /* Future logic for tooltips can go here. */ }
}