// Assets/Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Build,
    WaveInProgress,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public GameState currentState;

    [Header("Player Stats")]
    public int playerHealth = 20;
    public int playerMoney = 100;

    [Header("Dependencies")]
    public WaveManager waveManager;
    public ChoiceManager choiceManager;
    public UIManager uiManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Subscribe to events
        waveManager.OnWaveCompleted += HandleWaveCompleted;

        // Start the game in the build phase
        ChangeState(GameState.Build);
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (waveManager != null)
        {
            waveManager.OnWaveCompleted -= HandleWaveCompleted;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Build:
                Debug.Log("Entering Build Phase. Ready for next wave.");
                uiManager.ShowStartWaveButton(true);
                break;
            case GameState.WaveInProgress:
                uiManager.ShowStartWaveButton(false);
                waveManager.StartNextWave();
                break;
            case GameState.GameOver:
                Debug.Log("Game Over!");
                Time.timeScale = 0; // Pause game
                uiManager.ShowGameOverPanel();
                break;
            case GameState.Victory:
                Debug.Log("Victory!");
                uiManager.ShowVictoryPanel();
                break;
        }
    }

    public void StartWave()
    {
        if (currentState == GameState.Build)
        {
            ChangeState(GameState.WaveInProgress);
        }
    }

    private void HandleWaveCompleted(int waveNumber)
    {
        // Check for victory condition
        if (waveManager.IsLastWave())
        {
            ChangeState(GameState.Victory);
            return;
        }

        // Award money for completing the wave
        playerMoney += 100 + (waveNumber * 10);
        uiManager.UpdatePlayerStats();

        // Trigger the upgrade choice system
        if (waveNumber % choiceManager.wavesPerUpgradeChoice == 0)
        {
            choiceManager.TriggerUpgradeChoice();
        }

        ChangeState(GameState.Build);
    }

    public void TakeDamage(int amount)
    {
        playerHealth -= amount;
        uiManager.UpdatePlayerStats();
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            ChangeState(GameState.GameOver);
        }
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        uiManager.UpdatePlayerStats();
    }

    public bool SpendMoney(int amount)
    {
        if (playerMoney >= amount)
        {
            playerMoney -= amount;
            uiManager.UpdatePlayerStats();
            return true;
        }
        return false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}