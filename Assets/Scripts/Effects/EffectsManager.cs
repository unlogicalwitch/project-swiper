using UnityEngine;
using CandyCoded.HapticFeedback;

/// <summary>
/// Central hub that maps game events to visual effects.
/// Add new effect components here as the game grows.
/// No existing classes need to change when adding new effects.
/// </summary>
public class EffectsManager : Singleton<EffectsManager>
{
    [Header("Camera Effects")]
    [SerializeField] private CameraShake cameraShake;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    [Header("VFX — Match")]
    [SerializeField] private GameObject matchVFXPrefab;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void OnEnable()
    {
        FallingSymbol.OnSymbolMissed += HandleSymbolMissed;
        FallingSymbol.OnSymbolMatched += HandleSymbolMatched;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        FallingSymbol.OnSymbolMissed -= HandleSymbolMissed;
        FallingSymbol.OnSymbolMatched -= HandleSymbolMatched;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    void HandleSymbolMissed(FallingSymbol symbol)
    {
        //cameraShake?.Shake(missShakeDuration, missShakeMagnitude);
    }

    void HandleSymbolMatched(FallingSymbol symbol)
    {
        if (matchVFXPrefab != null && symbol != null)
        {
            // Spawn match hit VFX 
            Vector3 spawnPos = new Vector3(
                symbol.transform.position.x,
                symbol.transform.position.y,
                0f);

            Instantiate(matchVFXPrefab, spawnPos, Quaternion.identity);
        }

        ShakeCamera();
    }

    void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
            cameraShake?.Shake(shakeDuration, shakeMagnitude);
    }

    public void ShakeCamera()
    {
        cameraShake?.Shake(shakeDuration, shakeMagnitude);
        if (SettingsManager.Instance.Vibration)
        {
            HapticFeedback.HeavyFeedback();
        }
    }
}
