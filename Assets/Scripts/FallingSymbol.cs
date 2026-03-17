using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class FallingSymbol : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────────────────────
    /// <summary>Raised when this symbol reaches the bottom without being matched.</summary>
    public static event Action<FallingSymbol> OnSymbolMissed;

    /// <summary>Raised when this symbol is successfully matched by the player.</summary>
    public static event Action<FallingSymbol> OnSymbolMatched;

    // ── Private state ─────────────────────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private GameConfig gameConfig;
    private GestureSO gestureData;
    private float fallSpeed;
    private bool matched = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(GestureSO gesture, GameConfig config, float fallSpeed)
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
    public void HandleMatched()
    {
        if (matched) return;
        matched = true;

        Debug.Log($"Symbol matched: {gestureData.gestureID}");
        OnSymbolMatched?.Invoke(this);
        AudioManager.Instance.PlaySFXRandomPitch("Swipe");

        StartCoroutine(MatchCoroutine());
    }

    void HandleMissed()
    {
        matched = true;
        Debug.Log($"Symbol missed: {gestureData?.gestureID}");
        OnSymbolMissed?.Invoke(this);
        this.gameObject.SetActive(false);
    }

    IEnumerator MatchCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        this.gameObject.SetActive(false);
    }

    public GestureSO GetGestureData() => gestureData;
}
