using System.Collections;
using UnityEngine;

/// <summary>
/// Pure camera shake component. Knows nothing about game events.
/// Call Shake(duration, magnitude) from EffectsManager to trigger.
/// Uses Perlin noise for smooth, organic movement.
/// The GestureTrail is unaffected because it renders on a separate
/// Trail Camera that is not subject to shake.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Triggers a shake. If already shaking, restarts with new parameters.
    /// </summary>
    /// <param name="duration">How long the shake lasts in seconds.</param>
    /// <param name="magnitude">Max offset in world units.</param>
    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        float seed = Random.value * 100f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float strength = magnitude * (1f - t); // ease out

            float offsetX = (Mathf.PerlinNoise(seed + elapsed * 20f, 0f) - 0.5f) * 2f * strength;
            float offsetY = (Mathf.PerlinNoise(0f, seed + elapsed * 20f) - 0.5f) * 2f * strength;

            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }
}
