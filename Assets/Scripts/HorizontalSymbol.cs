using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalSymbol : FallingSymbol
{
    public enum Direction { Left, Right }
    public Direction moveDirection;

    private float speed;

    // ── Overrides ─────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(GestureSO gesture, GameConfig config, float speed, ObjectPool objectPool)
    {
        gestureData = gesture;
        gameConfig = config;
        this.speed = speed;
        this.objectPool = objectPool;
        matched = false;

        // Randomly choose left or right movement direction
        moveDirection = (Random.value < 0.5f) ? Direction.Left : Direction.Right;

        if (spriteRenderer != null)
            spriteRenderer.sprite = gesture.gestureSprite;

        //Calculate spawn position based on device aspect ratio and move direction
        var spawnX = GameManager.Instance.WorldSpawnX;
        float randomY = UnityEngine.Random.Range(-1, Camera.main.orthographicSize - 1f);
        transform.position = new Vector3((moveDirection == Direction.Left) ? spawnX : -spawnX,randomY, 0f);
    }

    protected override void Update()
    {
        if (matched || gameConfig == null) return;

        // Move horizontally
        float directionVector = (moveDirection == Direction.Left) ? -1f : 1f;
        transform.position += Vector3.right * directionVector * speed * Time.deltaTime;

        if (transform.position.x < GameManager.Instance.WorldSpawnX * -1 - 1f || transform.position.x > GameManager.Instance.WorldSpawnX + 1f)
            HandleMissed();
    }
}

