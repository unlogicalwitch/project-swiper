using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Microsoft.VisualBasic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    //[Header("Sources")]
    //private AudioSource bgmSource;
    //private AudioSource sfxSource;

    [Header("Sound Library")]
    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize AudioSources for each Sound
            foreach (Sound s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.playOnAwake = s.playOnAwake;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s != null)
            s.source.Play();
        else
            Debug.LogWarning($"Sound '{name}' not found in AudioManager!");
    }

    public void PlaySFXRandomPitch(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            var originalPitch = s.source.pitch;
            s.source.pitch = Random.Range(originalPitch - 0.1f, originalPitch + 0.1f);
            s.source.Play();
        }
        else
            Debug.LogWarning($"Sound '{name}' not found in AudioManager!");
    }

    public void StopSFX(string name)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == name);
        if (s != null)
            s.source.Stop();
        else
            Debug.LogWarning($"Sound '{name}' not found in AudioManager!");
    }
}
