using UnityEngine;
using UnityEditor;

/// <summary>
/// One-shot editor utility: cleans up SymbolObjects and wires up
/// BossSymbolSlot + BossSequenceDisplay on the Boss hierarchy.
/// Run via Tools > Setup Boss Sequence.
/// </summary>
public static class SetupBossSequence
{
    [MenuItem("Tools/Setup Boss Sequence")]
    public static void Run()
    {
        // ── Find Boss root ────────────────────────────────────────────────────
        GameObject bossRoot = GameObject.Find("Boss");
        if (bossRoot == null) { Debug.LogError("Boss root not found!"); return; }

        Transform sequenceParent = bossRoot.transform.Find("SymbolSequence");
        if (sequenceParent == null) { Debug.LogError("Boss/SymbolSequence not found!"); return; }

        // ── Process each SymbolObject ─────────────────────────────────────────
        int childCount = sequenceParent.childCount;
        BossSymbolSlot[] slots = new BossSymbolSlot[childCount];

        for (int i = 0; i < childCount; i++)
        {
            GameObject slotObj = sequenceParent.GetChild(i).gameObject;

            // Remove unwanted gameplay components
            RemoveComponent<FallingSymbol>(slotObj);
            RemoveComponent<HorizontalSymbol>(slotObj);

            // Add BossSymbolSlot if not already present
            BossSymbolSlot slot = slotObj.GetComponent<BossSymbolSlot>();
            if (slot == null)
                slot = Undo.AddComponent<BossSymbolSlot>(slotObj);

            // Wire icon renderer (on the slot root)
            SpriteRenderer iconRenderer = slotObj.GetComponent<SpriteRenderer>();

            // Wire background renderer (on the Background child)
            Transform bgTransform = slotObj.transform.Find("Background");
            SpriteRenderer bgRenderer = bgTransform != null
                ? bgTransform.GetComponent<SpriteRenderer>()
                : null;

            // Use SerializedObject to set private serialized fields
            SerializedObject so = new SerializedObject(slot);
            so.FindProperty("iconRenderer").objectReferenceValue = iconRenderer;
            so.FindProperty("backgroundRenderer").objectReferenceValue = bgRenderer;
            so.ApplyModifiedProperties();

            slots[i] = slot;
        }

        // ── Add BossSequenceDisplay to Boss root ──────────────────────────────
        BossSequenceDisplay display = bossRoot.GetComponent<BossSequenceDisplay>();
        if (display == null)
            display = Undo.AddComponent<BossSequenceDisplay>(bossRoot);

        SpriteRenderer bossRenderer = bossRoot.GetComponent<SpriteRenderer>();

        SerializedObject displaySO = new SerializedObject(display);
        SerializedProperty slotsProp = displaySO.FindProperty("slots");
        slotsProp.arraySize = slots.Length;
        for (int i = 0; i < slots.Length; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        displaySO.FindProperty("bossRenderer").objectReferenceValue = bossRenderer;
        displaySO.ApplyModifiedProperties();

        // ── Add BossSequenceManager to GameManager ────────────────────────────
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj != null)
        {
            BossSequenceManager bsm = gmObj.GetComponent<BossSequenceManager>();
            if (bsm == null)
                bsm = Undo.AddComponent<BossSequenceManager>(gmObj);

            // Wire gameConfig and gestureLibrary from the existing GameManager
            GameManager gm = gmObj.GetComponent<GameManager>();
            if (gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                GameConfig config = gmSO.FindProperty("gameConfig").objectReferenceValue as GameConfig;
                GestureLibrary library = null;

                // Grab library from GestureManager
                GameObject gestMgrObj = GameObject.Find("GestureManager");
                if (gestMgrObj != null)
                {
                    SerializedObject gestSO = new SerializedObject(gestMgrObj.GetComponent<GestureManager>());
                    library = gestSO.FindProperty("gestureLibrary").objectReferenceValue as GestureLibrary;
                }

                SerializedObject bsmSO = new SerializedObject(bsm);
                bsmSO.FindProperty("gameConfig").objectReferenceValue = config;
                bsmSO.FindProperty("gestureLibrary").objectReferenceValue = library;
                bsmSO.ApplyModifiedProperties();
            }
        }

        EditorUtility.SetDirty(bossRoot);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"Boss Sequence setup complete. {childCount} slots configured.");
    }

    static void RemoveComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp != null)
        {
            Undo.DestroyObjectImmediate(comp);
            Debug.Log($"Removed {typeof(T).Name} from {go.name}");
        }
    }
}
