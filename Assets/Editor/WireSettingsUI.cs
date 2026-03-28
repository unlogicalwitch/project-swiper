using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class WireSettingsUI
{
    [MenuItem("Tools/Wire Settings UI")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // ── 1. Create SettingsManager GameObject (if not already present) ──────
        var existing = GameObject.Find("SettingsManager");
        if (existing == null)
        {
            var go = new GameObject("SettingsManager");
            go.AddComponent<SettingsManager>();
            Debug.Log("[WireSettingsUI] Created SettingsManager GameObject.");
        }
        else
        {
            if (existing.GetComponent<SettingsManager>() == null)
                existing.AddComponent<SettingsManager>();
            Debug.Log("[WireSettingsUI] SettingsManager already exists.");
        }

        // ── 2. Find the Settings container ────────────────────────────────────
        var settingsGO = GameObject.Find("Settings");
        if (settingsGO == null)
        {
            Debug.LogError("[WireSettingsUI] Could not find 'Settings' GameObject in scene.");
            return;
        }

        // ── 3. Add SettingsUI component (or reuse existing) ───────────────────
        var ui = settingsGO.GetComponent<SettingsUI>();
        if (ui == null)
            ui = settingsGO.AddComponent<SettingsUI>();

        // ── 4. Resolve references via SerializedObject ────────────────────────
        var so = new SerializedObject(ui);

        // SFX Slider
        var sfxSliderGO = GameObject.Find("SoundsSlider");
        if (sfxSliderGO != null)
            so.FindProperty("sfxSlider").objectReferenceValue = sfxSliderGO.GetComponent<Slider>();

        // Music Slider
        var musicSliderGO = GameObject.Find("MusicSlider");
        if (musicSliderGO != null)
            so.FindProperty("musicSlider").objectReferenceValue = musicSliderGO.GetComponent<Slider>();

        // Vibration Toggle
        var vibToggleGO = GameObject.Find("VibrationToggle");
        if (vibToggleGO != null)
            so.FindProperty("vibrationToggle").objectReferenceValue = vibToggleGO.GetComponent<Toggle>();

        // Close Button
        var closeButtonGO = GameObject.Find("CloseButton");
        if (closeButtonGO != null)
            so.FindProperty("closeButton").objectReferenceValue = closeButtonGO.GetComponent<Button>();

        // Settings Panel (the first SettingsPanel child — the background panel)
        so.FindProperty("settingsPanel").objectReferenceValue = settingsGO;

        so.ApplyModifiedProperties();

        // ── 5. Mark scene dirty and save ──────────────────────────────────────
        EditorUtility.SetDirty(ui);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("[WireSettingsUI] Done — SettingsUI wired successfully.");
    }
}
