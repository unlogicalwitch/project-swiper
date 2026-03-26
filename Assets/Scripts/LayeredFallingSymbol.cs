using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A falling symbol with multiple gesture layers.
/// Each successful match advances to the next layer.
/// Only the final layer match destroys the symbol and fires OnSymbolMatched.
/// Intermediate matches fire OnLayerCleared for effects/audio feedback.
/// </summary>
public class LayeredFallingSymbol : FallingSymbol
{
    // ── Event ─────────────────────────────────────────────────────────────────
    /// <summary>Raised when a non-final layer is cleared. Passes remaining layer count.</summary>
    public static event Action<LayeredFallingSymbol, int> OnLayerCleared;

    // ── Private state ─────────────────────────────────────────────────────────
    private GestureSO[] layers;
    private int currentLayerIndex = 0;

    // ── Init ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Initializes with a set of randomly picked unique gestures.
    /// The first gesture in the array is shown immediately.
    /// </summary>
    public void InitializeLayered(GestureSO[] randomLayers, GameConfig config, float fallSpeed, ObjectPool objectPool)
    {
        layers = randomLayers;
        currentLayerIndex = 0;
        this.objectPool = objectPool;

        // Initialize base with the first layer's gesture
        base.Initialize(layers[0], config, fallSpeed, objectPool);
    }

    // ── Overrides ─────────────────────────────────────────────────────────────

    /// <summary>Returns the gesture for the current active layer.</summary>
    public override GestureSO GetGestureData() => layers[currentLayerIndex];

    public override void HandleMatched()
    {
        if (matched) return;

        bool isFinalLayer = currentLayerIndex >= layers.Length - 1;

        if (isFinalLayer)
        {
            // Final layer — behave exactly like a normal symbol match
            base.HandleMatched();
        }
        else
        {
            // Advance to next layer
            currentLayerIndex++;
            gestureData = layers[currentLayerIndex];

            // Update sprite to show the new gesture
            if (spriteRenderer != null)
                spriteRenderer.sprite = gestureData.gestureSprite;

            AudioManager.Instance?.PlaySFXRandomPitch("Swipe");
            EffectsManager.Instance?.ShakeCamera();

            int remaining = layers.Length - currentLayerIndex;
            Debug.Log($"LayeredSymbol: layer cleared, {remaining} remaining.");
            OnLayerCleared?.Invoke(this, remaining);
        }
    }

    // ── Public helpers ────────────────────────────────────────────────────────

    public int CurrentLayer => currentLayerIndex;
    public int TotalLayers => layers != null ? layers.Length : 0;
    public int RemainingLayers => layers != null ? layers.Length - currentLayerIndex : 0;
}
