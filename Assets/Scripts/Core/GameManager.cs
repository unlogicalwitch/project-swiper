using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GestureManager symbolManager;
    [SerializeField] private HUDManager hudManager;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int currentHP;
    private int currentScore;
    private GameState currentState = GameState.GameOver;

    // ── Difficulty state ──────────────────────────────────────────────────────
    private float currentSpawnRate;
    private float currentFallSpeed;
    private Coroutine difficultyCoroutine;

    // ── Public accessors ──────────────────────────────────────────────────────
    public int CurrentHP => currentHP;
    public int CurrentScore => currentScore;
    public GameState CurrentState => currentState;
    public float WorldSpawnY;
    public float WorldSpawnX;

    /// <summary>Convenience — true only while actively playing.</summary>
    public bool IsGameActive => currentState == GameState.Playing;

    /// <summary>Raised whenever the game state changes.</summary>
    public static event Action<GameState> OnGameStateChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        WorldSpawnY = Camera.main.orthographicSize + gameConfig.spawnYPosition;
        WorldSpawnX = Camera.main.orthographicSize * Camera.main.aspect + gameConfig.spawnXPosition;
    }

    void OnEnable()
    {
        FallingSymbol.OnSymbolMissed += HandleSymbolMissed;
        FallingSymbol.OnSymbolMatched += HandleSymbolMatched;
    }

    void OnDisable()
    {
        FallingSymbol.OnSymbolMissed -= HandleSymbolMissed;
        FallingSymbol.OnSymbolMatched -= HandleSymbolMatched;
    }

    void Start()
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfig not assigned to GameManager!");
            return;
        }
        Debug.Log("Resolution: " + Screen.width + "x" + Screen.height);
        InitializeGame();
    }

    // ── Initialization ────────────────────────────────────────────────────────

    void InitializeGame()
    {
        currentHP = gameConfig.startingHP;
        currentScore = 0;
        currentSpawnRate = gameConfig.symbolSpawnRate;
        currentFallSpeed = gameConfig.symbolFallSpeed;

        hudManager?.UpdateHP(currentHP);
        hudManager?.UpdateScore(currentScore);

        SetState(GameState.Playing);
    }

    // ── State Machine ─────────────────────────────────────────────────────────

    void SetState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);

        switch (currentState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                StartSpawning();
                StartDifficultyRamp();
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                StopSpawning();
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                StopSpawning();
                StopDifficultyRamp();
                Debug.Log($"GAME OVER! Final score: {currentScore}");
                hudManager?.ShowGameOver(currentScore);
                break;
        }
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    void StartSpawning()
    {
        symbolManager?.StartSpawning(currentSpawnRate, currentFallSpeed);
    }

    void StopSpawning()
    {
        symbolManager?.StopSpawning();
    }

    // ── Difficulty Ramp ───────────────────────────────────────────────────────

    void StartDifficultyRamp()
    {
        if (difficultyCoroutine != null)
            StopCoroutine(difficultyCoroutine);

        difficultyCoroutine = StartCoroutine(DifficultyRampCoroutine());
    }

    void StopDifficultyRamp()
    {
        if (difficultyCoroutine != null)
        {
            StopCoroutine(difficultyCoroutine);
            difficultyCoroutine = null;
        }
    }

    IEnumerator DifficultyRampCoroutine()
    {
        while (currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(gameConfig.difficultyRampInterval);

            if (currentState != GameState.Playing) break;

            currentFallSpeed = Mathf.Min(
                currentFallSpeed + gameConfig.fallSpeedIncreasePerStep,
                gameConfig.maxFallSpeed);

            currentSpawnRate = Mathf.Max(
                currentSpawnRate - gameConfig.spawnRateDecreasePerStep,
                gameConfig.minSpawnRate);

            symbolManager?.UpdateSpawnParameters(currentSpawnRate, currentFallSpeed);

            Debug.Log($"Difficulty up — fall speed: {currentFallSpeed:F0}, spawn rate: {currentSpawnRate:F2}s");
            hudManager?.ShowDifficultyUp();
        }
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    void HandleSymbolMissed(FallingSymbol symbol)
    {
        if (currentState != GameState.Playing) return;

        currentHP--;
        hudManager?.UpdateHP(currentHP);

        if (currentHP <= 0)
            SetState(GameState.GameOver);
    }

    void HandleSymbolMatched(FallingSymbol symbol)
    {
        if (currentState != GameState.Playing) return;

        currentScore++;
        hudManager?.UpdateScore(currentScore);
        symbolManager?.TriggerPostMatchDelay();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void GameOver() => SetState(GameState.GameOver);

    public void PauseGame()
    {
        if (currentState == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
