using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns all player-facing settings (SFX volume, music volume, vibration).
/// Values are saved and loaded via PlayerPrefs so they persist between sessions.
/// Lives across scene loads (DontDestroyOnLoad) so every scene can access it.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static SettingsManager Instance { get; private set; }

    // ── PlayerPrefs keys ──────────────────────────────────────────────────────
    private const string KEY_SFX     = "settings_sfx_volume";
    private const string KEY_MUSIC   = "settings_music_volume";
    private const string KEY_VIBRATE = "settings_vibration";

    // ── Defaults ──────────────────────────────────────────────────────────────
    private const float DEFAULT_SFX   = 1f;
    private const float DEFAULT_MUSIC = 1f;
    private const bool  DEFAULT_VIB   = true;

    // ── Cached values (read-only from outside) ────────────────────────────────
    public float SFXVolume   { get; private set; }
    public float MusicVolume { get; private set; }
    public bool  Vibration   { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPlayerPrefs();
        // ApplyAll is deferred to Start so AudioManager has time to Awake first
    }

    private void Start()
    {
        ApplyAll();
        Debug.Log($"Settings loaded: SFX={SFXVolume}, Music={MusicVolume}, Vibration={Vibration}");
    }

    // ── Public setters (called by SettingsUI) ─────────────────────────────────

    /// <summary>Change SFX volume (0–1), save to PlayerPrefs, apply to AudioManager.</summary>
    public void SetSFXVolume(float volume)
    {
        SFXVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KEY_SFX, SFXVolume);
        PlayerPrefs.Save();
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
        Debug.Log($"SFX volume set to {SFXVolume}");
    }

    /// <summary>Change music volume (0–1), save to PlayerPrefs, apply to AudioManager.</summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(KEY_MUSIC, MusicVolume);
        PlayerPrefs.Save();
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
        Debug.Log($"Music volume set to {MusicVolume}");
    }

    /// <summary>Toggle vibration on/off, save to PlayerPrefs.</summary>
    public void SetVibration(bool enabled)
    {
        Vibration = enabled;
        PlayerPrefs.SetInt(KEY_VIBRATE, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Trigger a haptic vibration pulse — only fires if the player has
    /// vibration enabled. Call this from any gameplay event.
    /// </summary>
    public void TriggerVibration()
    {
        if (Vibration)
            Handheld.Vibrate();
    }

    /// <summary>
    /// Called by AudioManager after it finishes initialising its AudioSources,
    /// so that saved volume settings are applied immediately on scene load.
    /// </summary>
    public void ApplyAll()
    {
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>Read saved values from PlayerPrefs (falls back to defaults on first run).</summary>
    private void LoadFromPlayerPrefs()
    {
        SFXVolume   = PlayerPrefs.GetFloat(KEY_SFX,   DEFAULT_SFX);
        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_MUSIC);
        Vibration   = PlayerPrefs.GetInt(KEY_VIBRATE, DEFAULT_VIB ? 1 : 0) == 1;
    }
}
