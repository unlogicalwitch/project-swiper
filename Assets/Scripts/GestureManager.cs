using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all active falling symbols: spawning, matching, and post-match delay.
/// Raises OnActiveSymbolChanged so the HUD can display the current target hint.
/// </summary>
public class GestureManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;

    [Header("Symbols")]
    [SerializeField] private GestureLibrary gestureLibrary;

    [Header("Prefab")]
    [SerializeField] private ObjectPool symbolPool;

    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Raised whenever the primary target hint changes.
    /// Passes the new target GestureSO, or null when no symbols are active.
    /// </summary>
    public static event Action<GestureSO> OnActiveSymbolChanged;

    // ── Private state ─────────────────────────────────────────────────────────
    private readonly List<FallingSymbol> activeSymbols = new();
    private Coroutine spawnCoroutine;
    private float currentSpawnRate;
    private float currentFallSpeed;
    private bool postMatchDelayActive = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        GestureInput.OnGestureRecognized += HandleGestureRecognized;
        FallingSymbol.OnSymbolMissed += HandleSymbolRemoved;
        FallingSymbol.OnSymbolMatched += HandleSymbolRemoved;
    }

    void OnDisable()
    {
        GestureInput.OnGestureRecognized -= HandleGestureRecognized;
        FallingSymbol.OnSymbolMissed -= HandleSymbolRemoved;
        FallingSymbol.OnSymbolMatched -= HandleSymbolRemoved;
    }

    void Start()
    {
        if (gameConfig == null)
            Debug.LogError("GameConfig not assigned to GestureManager!");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Starts the spawn loop with the given rate and fall speed.</summary>
    public void StartSpawning(float spawnRate, float fallSpeed)
    {
        currentSpawnRate = spawnRate;
        currentFallSpeed = fallSpeed;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    /// <summary>Updates spawn rate and fall speed mid-game (difficulty ramp).</summary>
    public void UpdateSpawnParameters(float spawnRate, float fallSpeed)
    {
        currentSpawnRate = spawnRate;
        currentFallSpeed = fallSpeed;
    }

    /// <summary>Stops the spawn loop.</summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>Pauses spawning briefly after a successful match.</summary>
    public void TriggerPostMatchDelay()
    {
        StartCoroutine(PostMatchDelayCoroutine());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnRate);

            while (postMatchDelayActive)
                yield return null;

            if (activeSymbols.Count < gameConfig.maxConcurrentSymbols)
            {
                GestureSO gesture = gestureLibrary.GetRandomGesture();
                if (gesture != null)
                {
                    SpawnFallingSymbolObject(gesture, currentFallSpeed);
                    NotifyActiveSymbolChanged();
                }
            }
        }
    }

    IEnumerator PostMatchDelayCoroutine()
    {
        postMatchDelayActive = true;
        yield return new WaitForSeconds(gameConfig.nextSymbolSpawnDelay);
        postMatchDelayActive = false;
    }

    void SpawnFallingSymbolObject(GestureSO symbol, float fallSpeed)
    {
        if (symbolPool == null)
        {
            Debug.LogError("GestureManager: symbol pool not assigned!");
            return;
        }

        GameObject fallingObj = symbolPool.GetObject();
        FallingSymbol fallingSymbol = fallingObj.GetComponent<FallingSymbol>();

        if (fallingSymbol != null)
        {
            fallingSymbol.Initialize(symbol, gameConfig, fallSpeed);
            activeSymbols.Add(fallingSymbol);
        }
    }

    /// <summary>Fires OnActiveSymbolChanged with the current primary target (index 0), or null.</summary>
    void NotifyActiveSymbolChanged()
    {
        GestureSO target = (activeSymbols.Count > 0 && activeSymbols[0] != null)
            ? activeSymbols[0].GetGestureData()
            : null;

        OnActiveSymbolChanged?.Invoke(target);
    }

    void HandleSymbolRemoved(FallingSymbol symbol)
    {
        activeSymbols.Remove(symbol);
        NotifyActiveSymbolChanged();
    }

    void HandleGestureRecognized(string gestureName, float confidence)
    {
        if (activeSymbols.Count == 0) return;

        // Find ALL symbols matching the gesture — one draw clears every duplicate
        List<FallingSymbol> matches = activeSymbols.FindAll(s =>
            s != null &&
            s.GetGestureData().gestureID.Equals(gestureName, System.StringComparison.OrdinalIgnoreCase));

        if (matches.Count > 0)
        {
            foreach (FallingSymbol match in matches)
                match.HandleMatched();

            TriggerPostMatchDelay();
        }
        else
        {
            Debug.Log($"GestureManager: '{gestureName}' does not match any active symbol.");
        }
    }
}
