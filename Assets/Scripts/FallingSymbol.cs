using System;
using System.Collections;
using UnityEngine;

public class FallingSymbol : MonoBehaviour
{
    public static event Action<FallingSymbol> OnSymbolMissed;
    public static event Action<FallingSymbol> OnSymbolMatched;

    protected SpriteRenderer spriteRenderer;
    protected BoilingAnimator boilingAnimator;
    protected GameConfig gameConfig;
    protected GestureSO gestureData;
    protected float fallSpeed;
    protected bool matched = false;
    protected ObjectPool objectPool;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boilingAnimator = GetComponent<BoilingAnimator>();
    }

    public virtual void Initialize(GestureSO gesture, GameConfig config, float fallSpeed, ObjectPool objectPool)
    {
        gestureData = gesture;
        gameConfig = config;
        this.fallSpeed = fallSpeed;
        this.objectPool = objectPool;
        matched = false;

        if (boilingAnimator != null)
        {
            Debug.Log("set frames");
            boilingAnimator.SetFrames(gesture.boilingFrames);
        }
        else if (spriteRenderer != null)
            spriteRenderer.sprite = gesture.gestureSprite;

        var spawnY = GameManager.Instance.WorldSpawnY;
        float randomX = UnityEngine.Random.Range(-gameConfig.spawnXOffset, gameConfig.spawnXOffset);
        transform.position = new Vector3(randomX, spawnY, 0f);
    }

    protected virtual void Update()
    {
        if (matched || gameConfig == null) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < gameConfig.missYPosition)
            HandleMissed();
    }

    // Called externally when the player's gesture matches this symbol
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
        boilingAnimator?.Stop();
        Debug.Log($"Symbol missed: {gestureData?.gestureID}");
        OnSymbolMissed?.Invoke(this);
        objectPool.ReturnObject(gameObject);
    }

    IEnumerator MatchCoroutine()
    {
        boilingAnimator?.Stop();
        yield return new WaitForSeconds(0.1f);
        objectPool.ReturnObject(gameObject);
    }

    //Returns the gesture that must be drawn to match this symbol right now
    public virtual GestureSO GetGestureData() => gestureData;
}
