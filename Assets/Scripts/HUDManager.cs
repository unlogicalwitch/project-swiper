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
    }

    void OnDisable()
    {
        GestureManager.OnActiveSymbolChanged -= UpdateTargetHint;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (difficultyUpBanner != null)
            difficultyUpBanner.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(() => gameManager?.RestartGame());
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
