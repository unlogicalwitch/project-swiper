using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages a single boss-fight sequence:
///   1. Generates a random ordered list of gestures and fires OnSequenceStarted.
///   2. Waits for BossSequenceDisplay to call NotifyEnterComplete() (after the enter animation).
///   3. Fires OnTimerStarted, then runs the countdown.
///   4. Validates each player gesture in order.
///   5. Fires OnBossSequenceComplete(success) when done.
/// </summary>
public class BossSequenceManager : Singleton<BossSequenceManager>
{
    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired immediately when the sequence array is ready (before the timer starts).</summary>
    public static event Action<GestureSO[]> OnSequenceStarted;

    /// <summary>Fired after the enter animation completes — this is when the timer begins.</summary>
    public static event Action<GestureSO[]> OnTimerStarted;

    /// <summary>Fired each time the player correctly matches the current symbol. int = matched index.</summary>
    public static event Action<int> OnSymbolMatched;

    /// <summary>Fired when the player draws the wrong gesture.</summary>
    public static event Action OnWrongGesture;

    /// <summary>Fired every frame while the timer is running. float = 0..1 (1 = full time remaining).</summary>
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
    private bool enterComplete;
    private Coroutine sequenceCoroutine;

    // ── Public API ────────────────────────────────────────────────────────────

    public bool IsRunning => isRunning;

    /// <summary>
    /// Begins a new boss sequence. Fires OnSequenceStarted immediately so the
    /// display can start the enter animation. The timer does NOT start until
    /// NotifyEnterComplete() is called.
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

        GestureSO[] allGestures = gestureLibrary.GetAllGestures();
        sequence = new GestureSO[length];
        for (int i = 0; i < length; i++)
            sequence[i] = gestureLibrary.GetRandomGesture();

        currentIndex = 0;
        isRunning = true;
        enterComplete = false;

        // Notify display immediately so it can store the sequence for the animation
        OnSequenceStarted?.Invoke(sequence);

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    /// <summary>
    /// Called by BossSequenceDisplay once the enter animation finishes.
    /// This unblocks the timer and allows gesture input.
    /// </summary>
    public void NotifyEnterComplete()
    {
        enterComplete = true;
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
        // Wait until the enter animation signals it's done
        yield return new WaitUntil(() => enterComplete);

        // Now the timer starts — notify display to activate the first slot
        OnTimerStarted?.Invoke(sequence);
        GestureInput.OnGestureRecognized += HandleGestureRecognized;

        float elapsed = 0f;
        float duration = gameConfig.bossTimeDuration;

        while (elapsed < duration && isRunning)
        {
            elapsed += Time.deltaTime;
            float normalised = 1f - Mathf.Clamp01(elapsed / duration);
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
            AudioManager.Instance?.PlaySFXRandomPitch("Swipe");
            OnSymbolMatched?.Invoke(currentIndex);
            currentIndex++;

            if (currentIndex >= sequence.Length)
                StopSequenceInternal(success: true, fireEvent: true);
        }
        else
        {
            OnWrongGesture?.Invoke();
            //Debug.Log($"BossSequence: wrong gesture '{gestureName}', expected '{expected.gestureID}'");
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
