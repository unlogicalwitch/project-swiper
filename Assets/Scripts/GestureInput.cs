using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureInput : MonoBehaviour
{
    // ── Event ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Raised when the player completes a gesture that meets the confidence
    /// threshold. Subscribers receive the matched gesture ID and confidence score.
    /// </summary>
    public static event Action<string, float> OnGestureRecognized;

    // ── Inspector ─────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GestureLibrary gestureLibrary;
    [SerializeField] private GestureTrail trail;
    [SerializeField] private float minPointDistance = 6f;

    // ── Private state ─────────────────────────────────────────────────────────
    private readonly List<Vector2> points = new();
    private DollarRecognizer recognizer;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        if (gameConfig == null)
            Debug.LogError("GameConfig not assigned to GestureInput!");

        if (gestureLibrary == null)
            Debug.LogError("GestureLibrary not assigned to GestureInput!");

        recognizer = new DollarRecognizer();
        recognizer.Templates = gestureLibrary != null
            ? new List<GestureSO>(gestureLibrary.GetAllGestures())
            : new List<GestureSO>();
    }

    void Update()
    {
        if (gameManager == null || !gameManager.IsGameActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            points.Clear();
            trail.Begin();
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;

            if (points.Count == 0 ||
                Vector2.Distance(points[^1], pos) > minPointDistance)
            {
                points.Add(pos);
                trail.AddPoint(pos);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            trail.End();
            EvaluateGesture();
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    void EvaluateGesture()
    {
        if (!recognizer.Recognize(points, out string shape, out float confidence)
            || confidence < gameConfig.matchConfidenceThreshold)
        {
            Debug.Log("NO MATCH");
            return;
        }

        Debug.Log($"MATCH {shape} ({confidence:F2})");
        OnGestureRecognized?.Invoke(shape, confidence);
    }
}
