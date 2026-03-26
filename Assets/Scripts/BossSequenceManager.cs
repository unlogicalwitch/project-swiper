using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a single boss-fight sequence:
///   1. Generates a random ordered list of gestures.
///   2. Displays them in the HUD (via events).
///   3. Runs a countdown timer.
///   4. Validates each player gesture in order.
///   5. Fires OnBossSequenceComplete(success) when done.
///
/// Attach to any persistent GameObject (e.g. GameManager).
/// Call StartSequence() to begin; the manager cleans itself up on finish.
/// </summary>
public class BossSequenceManager : Singleton<BossSequenceManager>
{
    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when the sequence is fully set up and the timer starts.</summary>
    public static event Action<GestureSO[]> OnSequenceStarted;

    /// <summary>
    /// Fired each time the player correctly matches the current symbol.
    /// int = index of the symbol just matched (0-based).
    /// </summary>
    public static event Action<int> OnSymbolMatched;

    /// <summary>
    /// Fired when the player draws the wrong gesture.
    /// </summary>
    public static event Action OnWrongGesture;

    /// <summary>Fired every frame while the sequence is running. float = 0..1 (1 = full time remaining).</summary>
    public static event Action<float> OnTimerUpdated;

    /// <summary>Fired when the sequence ends. bool = true if the player won.</summary>
    public static event Action<bool> OnBossSequenceComplete;

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GestureLibrary gestureLibrary;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private GestureSO[] sequence;
    private int currentIndex;
    private bool isRunning;
    private Coroutine sequenceCoroutine;

    // ── Public API ────────────────────────────────────────────────────────────

    public bool IsRunning => isRunning;

    /// <summary>
    /// Begins a new boss sequence. Length is randomly chosen between
    /// gameConfig.bossSequenceLengthMin and bossSequenceLengthMax (inclusive).
    /// </summary>
    public void StartSequence()
    {
        if (isRunning)
        {
            Debug.LogWarning("BossSequenceManager: sequence already running!");
            return;
        }

        int length = UnityEngine.Random.Range(
            gameConfig.bossSequenceLengthMin,
            gameConfig.bossSequenceLengthMax + 1);

        // Build sequence — allow repeats so length is never capped by library size
        GestureSO[] allGestures = gestureLibrary.GetAllGestures();
        sequence = new GestureSO[length];
        for (int i = 0; i < length; i++)
            sequence[i] = allGestures[UnityEngine.Random.Range(0, allGestures.Length)];

        currentIndex = 0;
        isRunning = true;

        GestureInput.OnGestureRecognized += HandleGestureRecognized;

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    /// <summary>Force-aborts the sequence (e.g. on game over).</summary>
    public void AbortSequence()
    {
        if (!isRunning) return;
        StopSequenceInternal(success: false, fireEvent: false);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    IEnumerator RunSequence()
    {
        OnSequenceStarted?.Invoke(sequence);

        float elapsed = 0f;
        float limit = gameConfig.bossTimeLimit;

        while (elapsed < limit && isRunning)
        {
            elapsed += Time.deltaTime;
            float normalised = 1f - Mathf.Clamp01(elapsed / limit);
            OnTimerUpdated?.Invoke(normalised);
            yield return null;
        }

        // Timer ran out
        if (isRunning)
            StopSequenceInternal(success: false, fireEvent: true);
    }

    void HandleGestureRecognized(string gestureName, float confidence)
    {
        if (!isRunning || sequence == null) return;

        GestureSO expected = sequence[currentIndex];

        if (expected.gestureID.Equals(gestureName, StringComparison.OrdinalIgnoreCase))
        {
            // Correct gesture
            AudioManager.Instance?.PlaySFXRandomPitch("Swipe");
            OnSymbolMatched?.Invoke(currentIndex);
            currentIndex++;

            if (currentIndex >= sequence.Length)
            {
                // All symbols matched — success!
                StopSequenceInternal(success: true, fireEvent: true);
            }
        }
        else
        {
            // Wrong gesture
            OnWrongGesture?.Invoke();
            Debug.Log($"BossSequence: wrong gesture '{gestureName}', expected '{expected.gestureID}'");
        }
    }

    void StopSequenceInternal(bool success, bool fireEvent)
    {
        isRunning = false;
        GestureInput.OnGestureRecognized -= HandleGestureRecognized;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        OnTimerUpdated?.Invoke(success ? 1f : 0f);

        if (fireEvent)
            OnBossSequenceComplete?.Invoke(success);
    }
}
