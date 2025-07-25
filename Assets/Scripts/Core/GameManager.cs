// Scripts/Core/GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Build, WaveInProgress, Pause, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public GameState currentState;
    private GameState stateBeforePause;

    [Header("Player Stats")]
    public int playerMoney = 100;

    [Header("Dependencies")]
    public WaveManager waveManager;
    public ChoiceManager choiceManager;
    public UIManager uiManager;
    public HealthSystem baseHealthSystem;

    [Header("World Center Setup")]
    public PlayerController player;
    public WorldStreamer worldStreamer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // --- CORRECTED LOGIC TO CENTER THE PLAYER AND BASE ---
        if (worldStreamer != null && worldStreamer.chunkGenerator != null && player != null && baseHealthSystem != null)
        {
            // Get the REAL cell size from the generator
            Vector3 cellSize = worldStreamer.chunkGenerator.cellSize;

            // Calculate the total world size in world units
            float worldWidth = worldStreamer.worldSizeInChunks.x * worldStreamer.chunkGenerator.chunkSize.x * cellSize.x;
            float worldHeight = worldStreamer.worldSizeInChunks.y * worldStreamer.chunkGenerator.chunkSize.y * cellSize.y;

            // The center is half the total world size
            Vector3 worldCenter = new Vector3(worldWidth / 2.0f, worldHeight / 2.0f, 0);

            player.transform.position = worldCenter;
            baseHealthSystem.transform.position = worldCenter;

            Debug.Log($"World Center calculated at: {worldCenter}. Player and Base moved.");
        }
        // --- END OF CORRECTED LOGIC ---

        if (baseHealthSystem == null)
        {
            Debug.LogError("GameManager: BaseHealthSystem is not assigned! Game Over condition will not work.");
        }
        else
        {
            baseHealthSystem.OnDied += HandleGameOver;
        }

        waveManager.OnWaveCompleted += HandleWaveCompleted;
        ChangeState(GameState.Build);
    }

    // ... (rest of the script is identical)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    void OnDestroy()
    {
        if (waveManager != null) waveManager.OnWaveCompleted -= HandleWaveCompleted;
        if (baseHealthSystem != null) baseHealthSystem.OnDied -= HandleGameOver;
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        switch (currentState)
        {
            case GameState.Build:
                Time.timeScale = 1f;
                uiManager.UpdatePlayerStats();
                uiManager.ShowStartWaveButton(true);
                uiManager.SetPausePanelActive(false);
                break;
            case GameState.WaveInProgress:
                uiManager.ShowStartWaveButton(false);
                break;
            case GameState.Pause:
                stateBeforePause = currentState;
                Time.timeScale = 0f;
                uiManager.SetPausePanelActive(true);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                uiManager.ShowGameOverPanel();
                break;
            case GameState.Victory:
                Time.timeScale = 0f;
                uiManager.ShowVictoryPanel();
                break;
        }
    }

    public void TogglePause()
    {
        if (currentState != GameState.Pause && currentState != GameState.GameOver && currentState != GameState.Victory)
        {
            stateBeforePause = currentState;
            ChangeState(GameState.Pause);
        }
        else if (currentState == GameState.Pause)
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Pause) ChangeState(stateBeforePause);
    }

    public void StartWave()
    {
        if (currentState == GameState.Build) ChangeState(GameState.WaveInProgress);
    }

    private void HandleWaveCompleted(int waveNumber)
    {
        if (waveManager.IsLastWave())
        {
            ChangeState(GameState.Victory);
            return;
        }
        playerMoney += 100 + (waveNumber * 10);
        ChangeState(GameState.Build);
    }

    private void HandleGameOver()
    {
        ChangeState(GameState.GameOver);
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