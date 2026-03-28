using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bridges the Settings panel UI controls (sliders, toggle, close button)
/// to SettingsManager. Attach this to the Canvas/Settings GameObject.
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button closeButton;

    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Initialise UI controls from saved settings
        if (SettingsManager.Instance != null)
        {
            sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.SFXVolume);
            musicSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume);
            vibrationToggle.SetIsOnWithoutNotify(SettingsManager.Instance.Vibration);
        }

        // Wire up listeners
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        vibrationToggle.onValueChanged.RemoveListener(OnVibrationChanged);
        closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    // ── Callbacks ─────────────────────────────────────────────────────────────

    private void OnSFXChanged(float value)
    {
        SettingsManager.Instance?.SetSFXVolume(value);
    }

    private void OnMusicChanged(float value)
    {
        SettingsManager.Instance?.SetMusicVolume(value);
    }

    private void OnVibrationChanged(bool value)
    {
        SettingsManager.Instance?.SetVibration(value);
    }

    private void OnCloseClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
