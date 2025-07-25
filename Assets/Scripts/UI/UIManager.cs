// Assets/Scripts/UI/UIManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Services;

public class UIManager : MonoBehaviour
{
    [Header("Stat Displays")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI waveText;

    [Header("Buttons")]
    public Button startWaveButton;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject pausePanel;

    void Start()
    {
        if (startWaveButton != null)
        {
            // Note: StartWave method needs to be implemented in IGameManager
            // startWaveButton.onClick.AddListener(() => {
            //     var gameManager = ServiceLocator.GetService<IGameManager>();
            //     gameManager?.StartWave();
            // });
        }

        // Ensure all panels are hidden at the start of the game.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        // Note: These references need to be refactored to use proper service locator pattern
        // var gameManager = ServiceLocator.GetService<IGameManager>();
        // if (gameManager != null)
        // {
        //     // Source health directly from the base's HealthSystem via the GameManager.
        //     healthText.text = $"Health: {gameManager.GetBaseHealth()}";
        //     moneyText.text = $"Money: ${gameManager.GetPlayerMoney()}";
        //     waveText.text = $"Wave: {gameManager.GetCurrentWave()} / {gameManager.GetTotalWaves()}";
        // }
        
        // Placeholder values for now
        if (healthText != null) healthText.text = "Health: 100";
        if (moneyText != null) moneyText.text = "Money: $0";
        if (waveText != null) waveText.text = "Wave: 1 / 10";
    }

    public void ShowStartWaveButton(bool show)
    {
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(show);
        }
    }

    public void SetPausePanelActive(bool isActive)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(isActive);
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowVictoryPanel()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }
}