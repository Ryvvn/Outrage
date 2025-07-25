// Assets/Scripts/UI/UIManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
            startWaveButton.onClick.AddListener(GameManager.Instance.StartWave);
        }

        // Ensure all panels are hidden at the start of the game.
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        // Source health directly from the base's HealthSystem via the GameManager.
        if (GameManager.Instance.baseHealthSystem != null)
        {
            healthText.text = $"Health: {GameManager.Instance.baseHealthSystem.CurrentHealth}";
        }
        moneyText.text = $"Money: ${GameManager.Instance.playerMoney}";
        waveText.text = $"Wave: {GameManager.Instance.waveManager.currentWaveIndex + 1} / {GameManager.Instance.waveManager.waves.Count}";
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