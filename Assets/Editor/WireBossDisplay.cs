using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class WireBossDisplay
{
    [MenuItem("Tools/Wire Boss Display")]
    public static void Run()
    {
        GameObject bossManagerObj = GameObject.Find("BossManager");
        if (bossManagerObj == null) { Debug.LogError("BossManager not found!"); return; }

        BossSequenceDisplay display = bossManagerObj.GetComponent<BossSequenceDisplay>();
        if (display == null) { Debug.LogError("BossSequenceDisplay not found on BossManager!"); return; }

        // Find Boss root
        GameObject bossRoot = GameObject.Find("Boss");
        if (bossRoot == null) { Debug.LogError("Boss not found!"); return; }

        // Find SymbolSequence
        Transform sequenceRoot = bossRoot.transform.Find("SymbolSequence");
        if (sequenceRoot == null) { Debug.LogError("Boss/SymbolSequence not found!"); return; }

        // Collect slots
        int childCount = sequenceRoot.childCount;
        BossSymbolSlot[] slots = new BossSymbolSlot[childCount];
        for (int i = 0; i < childCount; i++)
            slots[i] = sequenceRoot.GetChild(i).GetComponent<BossSymbolSlot>();

        // Apply via SerializedObject
        SerializedObject so = new SerializedObject(display);
        so.FindProperty("bossTransform").objectReferenceValue   = bossRoot.transform;
        so.FindProperty("bossRenderer").objectReferenceValue    = bossRoot.GetComponent<SpriteRenderer>();
        so.FindProperty("sequenceRoot").objectReferenceValue    = sequenceRoot;

        SerializedProperty slotsProp = so.FindProperty("slots");
        slotsProp.arraySize = slots.Length;
        for (int i = 0; i < slots.Length; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(bossManagerObj);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log($"BossSequenceDisplay wired: bossTransform=Boss, sequenceRoot=SymbolSequence, {slots.Length} slots.");
    }
}
