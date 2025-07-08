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

    void Start()
    {
        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(GameManager.Instance.StartWave);
        }

        //gameOverPanel.SetActive(false);
        //victoryPanel.SetActive(false);
        UpdatePlayerStats();
    }

    public void UpdatePlayerStats()
    {
        healthText.text = $"Health: {GameManager.Instance.playerHealth}";
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

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowVictoryPanel()
    {
        victoryPanel.SetActive(true);
    }
}