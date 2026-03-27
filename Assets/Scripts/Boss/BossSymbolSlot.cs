
using UnityEngine;

/// <summary>
/// Attached to each SymbolObject under Boss/SymbolSequence.
/// Purely a display component — no gameplay logic.
/// </summary>
public class BossSymbolSlot : MonoBehaviour
{
    public enum SlotState { Hidden, Pending, Active, Matched }

    [Header("Renderers")]
    [SerializeField] private SpriteRenderer iconRenderer;
    private SpriteRenderer backgroundRenderer;

    [Header("Colors")]
    [SerializeField] private Color pendingColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    [SerializeField] private Color activeColor  = Color.white;
    [SerializeField] private Color matchedColor = new Color(0.3f, 1f, 0.4f, 1f);

    private void Awake()
    {
        iconRenderer = GetComponent<SpriteRenderer>();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void SetGesture(GestureSO gesture)
    {
        if (iconRenderer != null)
            iconRenderer.sprite =  gesture.gestureSprite;
    }

    public void SetState(SlotState state)
    {
        switch (state)
        {
            case SlotState.Hidden:
                gameObject.SetActive(false);
                break;

            case SlotState.Pending:
                gameObject.SetActive(true);
                ApplyColor(pendingColor);
                break;

            case SlotState.Active:
                gameObject.SetActive(true);
                ApplyColor(activeColor);
                break;

            case SlotState.Matched:
                gameObject.SetActive(true);
                ApplyColor(matchedColor);
                break;
        }
    }

    // ── Private ───────────────────────────────────────────────────────────────

    void ApplyColor(Color color)
    {
        if (iconRenderer != null)
            iconRenderer.color = color;

        if (backgroundRenderer != null)
        {
            backgroundRenderer.gameObject.SetActive(true);
            backgroundRenderer.color = color;
        }
    }
}
