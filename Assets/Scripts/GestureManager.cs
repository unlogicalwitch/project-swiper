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

    [Header("Prefabs")]
    [SerializeField] private ObjectPool symbolPool;
    [SerializeField] private ObjectPool layeredSymbolPool;

    //  Events 
    // <summary>
    // Raised whenever the primary target hint changes.
    // Passes the new target GestureSO, or null when no symbols are active.
    // </summary>
    public static event Action<GestureSO> OnActiveSymbolChanged;

    //  Private state 
    private readonly List<FallingSymbol> activeSymbols = new();
    private Coroutine spawnCoroutine;
    private Coroutine horizontalSpawnCoroutine;
    private float currentSpawnRate;
    private float currentFallSpeed;
    private bool postMatchDelayActive = false;

    //  Lifecycle 
    void OnEnable()
    {
        GestureInput.OnGestureRecognized += HandleGestureRecognized;
        FallingSymbol.OnSymbolMissed += HandleSymbolRemoved;
        FallingSymbol.OnSymbolMatched += HandleSymbolRemoved;
        LayeredFallingSymbol.OnLayerCleared += HandleLayerCleared;
    }

    void OnDisable()
    {
        GestureInput.OnGestureRecognized -= HandleGestureRecognized;
        FallingSymbol.OnSymbolMissed -= HandleSymbolRemoved;
        FallingSymbol.OnSymbolMatched -= HandleSymbolRemoved;
        LayeredFallingSymbol.OnLayerCleared -= HandleLayerCleared;
    }

    void Start()
    {
        if (gameConfig == null)
            Debug.LogError("GameConfig not assigned to GestureManager!");
    }

    //  Public API 

    /// <summary>Starts the spawn loop with the given rate and fall speed.</summary>
    public void StartSpawning(float spawnRate, float fallSpeed)
    {
        currentSpawnRate = spawnRate;
        currentFallSpeed = fallSpeed;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnCoroutine());

        horizontalSpawnCoroutine = StartCoroutine(SpawnHorizontalSymbolCoroutine());
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
            StopCoroutine(horizontalSpawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>Pauses spawning briefly after a successful match.</summary>
    public void TriggerPostMatchDelay()
    {
        //StartCoroutine(PostMatchDelayCoroutine());
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
                bool spawnLayered = layeredSymbolPool != null
                    && UnityEngine.Random.value < gameConfig.layeredSymbolChance;

                if (spawnLayered)
                    SpawnLayeredSymbolObject(currentFallSpeed);
                else
                {
                    GestureSO gesture = gestureLibrary.GetRandomGesture();
                    if (gesture != null)
                        SpawnFallingSymbolObject(gesture, currentFallSpeed);
                }

                NotifyActiveSymbolChanged();
            }
        }
    }

    IEnumerator SpawnHorizontalSymbolCoroutine()
    {
        while (true)
        {
            float cooldown = UnityEngine.Random.Range(
                gameConfig.horizontalSymbolMinSpawnRate,
                gameConfig.horizontalSymbolMaxSpawnRate);

            yield return new WaitForSeconds(cooldown);

            SpawnHorizontalSymbolObject(gameConfig.horizontalSymbolSpeed);
        }
    }

    IEnumerator PostMatchDelayCoroutine()
    {
        postMatchDelayActive = true;
        yield return new WaitForSeconds(gameConfig.nextSymbolSpawnDelay);
        postMatchDelayActive = false;
    }
    
    // SPAWNING
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

    void SpawnLayeredSymbolObject(float fallSpeed)
    {
        if (layeredSymbolPool == null) return;

        GestureSO[] layers = gestureLibrary.GetUniqueRandomGestures(gameConfig.layeredSymbolLayers);
        if (layers == null || layers.Length == 0) return;

        GameObject fallingObj = layeredSymbolPool.GetObject();
        LayeredFallingSymbol layered = fallingObj.GetComponent<LayeredFallingSymbol>();

        if (layered != null)
        {
            layered.InitializeLayered(layers, gameConfig, fallSpeed);
            activeSymbols.Add(layered);
        }
    }

    void SpawnHorizontalSymbolObject(float speed)
    {
        if (symbolPool == null)
        {
            Debug.LogError("GestureManager: symbol pool not assigned!");
            return;
        }

        GameObject fallingObj = symbolPool.GetObject();
        HorizontalSymbol horizontalSymbol = fallingObj.GetComponent<HorizontalSymbol>();

        if (horizontalSymbol != null)
        {
            GestureSO gesture = gestureLibrary.GetRandomGesture();
            horizontalSymbol.Initialize(gesture, gameConfig, speed);
            activeSymbols.Add(horizontalSymbol);
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

    void HandleLayerCleared(LayeredFallingSymbol symbol, int remaining)
    {
        // Notify HUD hint in case the active symbol's gesture just changed
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
