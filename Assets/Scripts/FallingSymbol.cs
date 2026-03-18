using System;
using System.Collections;
using UnityEngine;

public class FallingSymbol : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Raised when this symbol reaches the bottom without being matched.</summary>
    public static event Action<FallingSymbol> OnSymbolMissed;

    /// <summary>Raised when this symbol is successfully matched by the player.</summary>
    public static event Action<FallingSymbol> OnSymbolMatched;

    // ── Protected state (accessible to subclasses) ────────────────────────────
    protected SpriteRenderer spriteRenderer;
    protected GameConfig gameConfig;
    protected GestureSO gestureData;
    protected float fallSpeed;
    protected bool matched = false;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(GestureSO gesture, GameConfig config, float fallSpeed)
    {
        gestureData = gesture;
        gameConfig = config;
        this.fallSpeed = fallSpeed;
        matched = false;

        if (spriteRenderer != null)
            spriteRenderer.sprite = gesture.gestureSprite;

        float randomX = UnityEngine.Random.Range(-gameConfig.spawnXOffset, gameConfig.spawnXOffset);
        transform.position = new Vector3(randomX, gameConfig.spawnYPosition, 0f);
    }

    void Update()
    {
        if (matched || gameConfig == null) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < gameConfig.missYPosition)
            HandleMissed();
    }

    /// <summary>Called externally when the player's gesture matches this symbol.</summary>
    public virtual void HandleMatched()
    {
        if (matched) return;
        matched = true;

        Debug.Log($"Symbol matched: {gestureData.gestureID}");
        OnSymbolMatched?.Invoke(this);
        AudioManager.Instance?.PlaySFXRandomPitch("Swipe");
        

        StartCoroutine(MatchCoroutine());
    }

    protected void HandleMissed()
    {
        matched = true;
        Debug.Log($"Symbol missed: {gestureData?.gestureID}");
        OnSymbolMissed?.Invoke(this);
        gameObject.SetActive(false);
    }

    IEnumerator MatchCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }

    /// <summary>Returns the gesture that must be drawn to match this symbol right now.</summary>
    public virtual GestureSO GetGestureData() => gestureData;
}
