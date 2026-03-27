using System.Collections;
using UnityEngine;

/// <summary>
/// Lives on BossManager. Drives the full boss enter animation and
/// all visual feedback for the sequence (slot states, boss color).
///
/// Enter sequence:
///   1. Boss slides down from off-screen to its resting position (DOTween-free, manual lerp).
///   2. Symbol slots pop in one by one.
///   3. Notifies BossSequenceManager to start the timer.
/// </summary>
public class BossSequenceDisplay : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Boss Transform & Renderer")]
    [SerializeField] private Transform bossTransform;
    [SerializeField] private SpriteRenderer bossRenderer;

    [Header("Symbol Sequence Root")]
    [SerializeField] private Transform sequenceRoot;   // Boss/SymbolSequence

    [Header("Slots")]
    [SerializeField] private BossSymbolSlot[] slots;

    [Header("Enter Animation")]
    [SerializeField] private float enterOffscreenY   = 8f;    // world Y to start from
    [SerializeField] private float enterDuration     = 0.6f;  // seconds to slide down
    [SerializeField] private float slotPopInterval   = 0.06f; // seconds between each slot pop-in
    [SerializeField] private AnimationCurve enterCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Colors")]
    [SerializeField] private Color idleColor    = Color.white;
    [SerializeField] private Color activeColor  = new Color(1f, 0.4f, 0.2f, 1f);
    [SerializeField] private Color successColor = new Color(0.3f, 1f, 0.4f, 1f);
    [SerializeField] private Color failColor    = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Wrong Gesture Flash")]
    [SerializeField] private Color wrongFlashColor  = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private float wrongFlashDuration = 0.15f;

    // ── Private state ─────────────────────────────────────────────────────────

    private Vector3 bossRestPosition;   // recorded in Awake from the scene position
    private GestureSO[] pendingSequence; // stored from OnSequenceStarted until slots are shown

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Record the designer-placed resting position
        if (bossTransform != null)
            bossRestPosition = bossTransform.position;

        // Hide everything at start
        if (bossTransform != null)
            bossTransform.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        BossSequenceManager.OnSequenceStarted      += HandleSequenceStarted;
        BossSequenceManager.OnTimerStarted         += HandleTimerStarted;
        BossSequenceManager.OnSymbolMatched        += HandleSymbolMatched;
        BossSequenceManager.OnWrongGesture         += HandleWrongGesture;
        BossSequenceManager.OnBossSequenceComplete += HandleSequenceComplete;
        GameManager.OnGameStateChanged             += HandleGameStateChanged;
    }

    void OnDisable()
    {
        BossSequenceManager.OnSequenceStarted      -= HandleSequenceStarted;
        BossSequenceManager.OnTimerStarted         -= HandleTimerStarted;
        BossSequenceManager.OnSymbolMatched        -= HandleSymbolMatched;
        BossSequenceManager.OnWrongGesture         -= HandleWrongGesture;
        BossSequenceManager.OnBossSequenceComplete -= HandleSequenceComplete;
        GameManager.OnGameStateChanged             -= HandleGameStateChanged;
    }

    // ── Game State ────────────────────────────────────────────────────────────

    void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.BossFight)
        {
            // Show the boss object and kick off the enter animation.
            // BossSequenceManager.StartSequence() has already been called by GameManager,
            // which fires OnSequenceStarted — we store the sequence and begin the animation.
            if (bossTransform != null)
            {
                bossTransform.gameObject.SetActive(true);
                bossTransform.position = new Vector3(
                    bossRestPosition.x,
                    enterOffscreenY,
                    bossRestPosition.z);
            }

            SetBossColor(idleColor);
            HideAllSlots();
            StartCoroutine(EnterSequenceCoroutine());
        }
        else if (state == GameState.Playing || state == GameState.GameOver)
        {
            StartCoroutine(ExitCoroutine());
        }
    }

    // ── Enter Animation ───────────────────────────────────────────────────────

    IEnumerator EnterSequenceCoroutine()
    {
        // 1. Slide boss down to resting position
        yield return StartCoroutine(SlideBossIn());

        // 2. Flash to active color
        SetBossColor(activeColor);

        // 3. Wait for OnSequenceStarted to have fired (it fires before this coroutine
        //    in most cases since StartSequence is called synchronously, but guard anyway)
        float waited = 0f;
        while (pendingSequence == null && waited < 2f)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        if (pendingSequence == null)
        {
            Debug.LogWarning("BossSequenceDisplay: no sequence received, skipping slot pop-in.");
            BossSequenceManager.Instance?.NotifyEnterComplete();
            yield break;
        }

        // 4. Pop slots in one by one
        yield return StartCoroutine(PopSlotsIn(pendingSequence));
        pendingSequence = null;

        // 5. Tell BossSequenceManager the animation is done — start the timer now
        BossSequenceManager.Instance?.NotifyEnterComplete();
    }

    IEnumerator SlideBossIn()
    {
        if (bossTransform == null) yield break;

        Vector3 startPos = bossTransform.position;
        float elapsed = 0f;

        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = enterCurve.Evaluate(Mathf.Clamp01(elapsed / enterDuration));
            bossTransform.position = Vector3.LerpUnclamped(startPos, bossRestPosition, t);
            yield return null;
        }

        bossTransform.position = bossRestPosition;
    }

    IEnumerator PopSlotsIn(GestureSO[] sequence)
    {
        // Show the sequence root first
        if (sequenceRoot != null)
            sequenceRoot.gameObject.SetActive(true);

        int slotCount = slots != null ? slots.Length : 0;

        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) continue;

            if (i < sequence.Length)
            {
                // All start as pending; first will be activated after timer begins
                slots[i].SetState(BossSymbolSlot.SlotState.Pending);
                slots[i].SetGesture(sequence[i]);
            }
            else
            {
                slots[i].SetState(BossSymbolSlot.SlotState.Hidden);
            }

            yield return new WaitForSeconds(slotPopInterval);
        }

        // Small pause before timer starts
        yield return new WaitForSeconds(0.2f);
    }

    // ── Sequence Events ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by BossSequenceManager as soon as the sequence array is ready.
    /// We store it here and use it during the enter animation.
    /// </summary>
    void HandleSequenceStarted(GestureSO[] sequence)
    {
        pendingSequence = sequence;
    }

    /// <summary>Called by BossSequenceManager once the timer actually starts.</summary>
    public void HandleTimerStarted(GestureSO[] sequence)
    {
        // Activate the first slot now that the player can start drawing
        if (slots != null && slots.Length > 0 && slots[0] != null)
            slots[0].SetState(BossSymbolSlot.SlotState.Active);
    }

    void HandleSymbolMatched(int matchedIndex)
    {
        if (slots == null) return;

        if (matchedIndex < slots.Length && slots[matchedIndex] != null)
            slots[matchedIndex].SetState(BossSymbolSlot.SlotState.Matched);

        int next = matchedIndex + 1;
        if (next < slots.Length && slots[next] != null
            && slots[next].gameObject.activeSelf)
        {
            slots[next].SetState(BossSymbolSlot.SlotState.Active);
        }

        EffectsManager.Instance.ShakeCamera();
    }

    void HandleWrongGesture()
    {
        StartCoroutine(WrongFlashCoroutine());
    }

    void HandleSequenceComplete(bool success)
    {
        SetBossColor(success ? successColor : failColor);
    }

    // ── Exit ──────────────────────────────────────────────────────────────────

    IEnumerator ExitCoroutine()
    {
        // Let the result color linger briefly
        yield return new WaitForSeconds(1.5f);
        SetBossColor(idleColor);
        HideAllSlots();
        if (bossTransform != null)
            bossTransform.gameObject.SetActive(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetBossColor(Color color)
    {
        if (bossRenderer != null)
            bossRenderer.color = color;
    }

    void HideAllSlots()
    {
        if (sequenceRoot != null)
            sequenceRoot.gameObject.SetActive(false);

        if (slots == null) return;
        foreach (var slot in slots)
            slot?.SetState(BossSymbolSlot.SlotState.Hidden);
    }

    IEnumerator WrongFlashCoroutine()
    {
        SetBossColor(wrongFlashColor);
        yield return new WaitForSeconds(wrongFlashDuration);
        SetBossColor(activeColor);
    }
}
