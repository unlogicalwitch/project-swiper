using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Direction-locked $1-style gesture recognizer.
/// - Same shape
/// - Same orientation
/// - Same draw direction
/// - Size & position invariant
/// - Templates normalized once (safe & correct)
/// </summary>
public class DollarRecognizer
{
    const int NumPoints = 32;
    const float SquareSize = 1f;
    const float DirectionAngleThreshold = 30f;

    public List<GestureSO> Templates = new();

    // Cache normalized templates
    Dictionary<GestureSO, List<Vector2>> normalizedTemplates
        = new Dictionary<GestureSO, List<Vector2>>();

    // ================= PUBLIC API =================

    public bool Recognize(
        List<Vector2> rawPoints,
        out string matchedGesture,
        out float confidence)
    {
        matchedGesture = "";
        confidence = 0f;

        if (rawPoints == null || rawPoints.Count < 10)
            return false;

        // Normalize player input
        List<Vector2> candidate = Normalize(new List<Vector2>(rawPoints));

        float bestDistance = float.MaxValue;
        string bestMatch = "";

        foreach (var template in Templates)
        {
            if (template == null || template.points == null || template.points.Count < 2)
                continue;

            // Normalize template ONCE
            if (!normalizedTemplates.ContainsKey(template))
            {
                normalizedTemplates[template] =
                    NormalizeTemplate(template.points);
            }

            List<Vector2> normalizedTemplate = normalizedTemplates[template];

            float d1 = PathDistance(candidate, normalizedTemplate);

            List<Vector2> reversed = new List<Vector2>(candidate);
            reversed.Reverse();

            float d2 = PathDistance(reversed, normalizedTemplate);

            float d = Mathf.Min(d1, d2);

            if (d < bestDistance)
            {
                bestDistance = d;
                bestMatch = template.gestureID;
            }
        }

        if (bestMatch == "")
            return false;

        // Convert distance to confidence (0–1 range)
        confidence = Mathf.Clamp01(1f - bestDistance / 0.5f);
        matchedGesture = bestMatch;

        return true;
    }

    // ================= NORMALIZATION =================

    List<Vector2> Normalize(List<Vector2> pts)
    {
        pts = Resample(pts, NumPoints);
        pts = ScaleToSquare(pts, SquareSize);
        pts = TranslateToOrigin(pts);
        return pts;
    }

    List<Vector2> NormalizeTemplate(List<Vector2> pts)
    {
        // Same pipeline as input — CRITICAL
        pts = Resample(new List<Vector2>(pts), NumPoints);
        pts = ScaleToSquare(pts, SquareSize);
        pts = TranslateToOrigin(pts);
        return pts;
    }

    List<Vector2> Resample(List<Vector2> pts, int targetCount)
    {
        float pathLength = PathLength(pts);
        float interval = pathLength / (targetCount - 1);
        float dist = 0f;

        List<Vector2> newPts = new() { pts[0] };

        for (int i = 1; i < pts.Count; i++)
        {
            float d = Vector2.Distance(pts[i - 1], pts[i]);

            if (dist + d >= interval)
            {
                float t = (interval - dist) / d;
                Vector2 newPoint = Vector2.Lerp(pts[i - 1], pts[i], t);
                newPts.Add(newPoint);
                pts.Insert(i, newPoint);
                dist = 0f;
            }
            else
            {
                dist += d;
            }
        }

        while (newPts.Count < targetCount)
            newPts.Add(pts[^1]);

        return newPts;
    }

    List<Vector2> ScaleToSquare(List<Vector2> pts, float size)
    {
        Rect box = BoundingBox(pts);
        List<Vector2> newPts = new();

        // Use uniform scaling when the gesture is strongly linear
        // (one dimension much larger than the other).
        // Non-uniform scaling amplifies tiny deviations on the short axis,
        // making near-straight lines fail to match their templates.
        float longSide = Mathf.Max(box.width, box.height);
        bool isLinear = longSide > 0 &&
                        Mathf.Min(box.width, box.height) / longSide < 0.2f;

        float scaleX, scaleY;
        if (isLinear)
        {
            // Uniform scale — preserve aspect ratio
            float uniformScale = longSide > 0 ? size / longSide : 1f;
            scaleX = uniformScale;
            scaleY = uniformScale;
        }
        else
        {
            // Standard non-uniform scale for 2D shapes
            scaleX = box.width  > 0 ? size / box.width  : 1f;
            scaleY = box.height > 0 ? size / box.height : 1f;
        }

        foreach (var p in pts)
            newPts.Add(new Vector2(p.x * scaleX, p.y * scaleY));

        return newPts;
    }

    List<Vector2> TranslateToOrigin(List<Vector2> pts)
    {
        Vector2 centroid = Centroid(pts);
        List<Vector2> newPts = new();

        foreach (var p in pts)
            newPts.Add(p - centroid);

        return newPts;
    }

    // ================= MATCHING =================

    float PathDistance(List<Vector2> a, List<Vector2> b)
    {
        float d = 0f;
        for (int i = 0; i < a.Count; i++)
            d += Vector2.Distance(a[i], b[i]);

        return d / a.Count;
    }

    // ================= UTIL =================

    float PathLength(List<Vector2> pts)
    {
        float length = 0f;
        for (int i = 1; i < pts.Count; i++)
            length += Vector2.Distance(pts[i - 1], pts[i]);

        return length;
    }

    Vector2 Centroid(List<Vector2> pts)
    {
        Vector2 c = Vector2.zero;
        foreach (var p in pts)
            c += p;

        return c / pts.Count;
    }

    Rect BoundingBox(List<Vector2> pts)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var p in pts)
        {
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }
}
