using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image[] hpHearts;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Target Hint")]
    [SerializeField] private Image targetHintImage;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button homeButton;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    [Header("Difficulty Flash")]
    [SerializeField] private GameObject difficultyUpBanner;
    [SerializeField] private float difficultyBannerDuration = 1.5f;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        GestureManager.OnActiveSymbolChanged += UpdateTargetHint;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GestureManager.OnActiveSymbolChanged -= UpdateTargetHint;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (difficultyUpBanner != null)
            difficultyUpBanner.SetActive(false);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void UpdateHP(int currentHP)
    {
        if (hpText != null)
            hpText.text = $"{currentHP}";

        if (hpHearts != null && hpHearts.Length > 0)
        {
            for (int i = 0; i < hpHearts.Length; i++)
            {
                if (hpHearts[i] != null)
                    hpHearts[i].enabled = i < currentHP;
            }
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    public void ShowGameOver(int finalScore)
    {
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {finalScore}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void ShowDifficultyUp()
    {
        if (difficultyUpBanner != null)
            StartCoroutine(FlashBanner());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                ShowPauseMenu();
                break;
            case GameState.Playing:
                HidePauseMenu();
                break;
        }
    }

    /// <summary>Updates the target hint image when the active symbol changes.</summary>
    void UpdateTargetHint(GestureSO gesture)
    {
        if (targetHintImage == null) return;
        targetHintImage.sprite = gesture != null ? gesture.gestureSprite : null;
    }

    IEnumerator FlashBanner()
    {
        difficultyUpBanner.SetActive(true);
        yield return new WaitForSeconds(difficultyBannerDuration);
        difficultyUpBanner.SetActive(false);
    }
}
