using UnityEngine;

/// <summary>
/// Cycles through a set of sprite frames to produce a hand-drawn "boiling" effect.
/// All instances share the same Time.time clock, so every symbol stays frame-in-sync
/// with no coroutines and minimal per-instance cost (one modulo + array lookup per Update).
/// Falls back to a still sprite when no boiling frames are assigned.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BoilingAnimator : MonoBehaviour
{
    [Tooltip("How many frames per second to cycle through the boiling frames.")]
    [SerializeField] private float fps;

    private Sprite[] frames;
    private SpriteRenderer spriteRenderer;
    private bool isPlaying;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Assign the boiling frames and the still fallback sprite, then start animating.
    /// If boilingFrames is null or empty, shows stillSprite as a static image.
    /// </summary>
    public void SetFrames(Sprite[] boilingFrames)
    {
        frames = boilingFrames;
        bool hasFrames = frames != null && frames.Length > 0;
        isPlaying = hasFrames;

        // Always assign a visible sprite immediately — never leave the renderer in a stale state
        spriteRenderer.sprite = frames?[0];
    }

    public void Stop()
    {
        isPlaying = false;
    }

    private void Update()
    {
        if (!isPlaying) return;

        // All instances read the same Time.time → all symbols stay in sync
        int index = Mathf.FloorToInt(Time.time * fps) % frames.Length;
        spriteRenderer.sprite = frames[index];
    }
}
