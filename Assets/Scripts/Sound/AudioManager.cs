using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxPlayerSource;
    public AudioSource sfxOtherSource;
    public AudioSource musicSource;
    public AudioSource uiSource;

    [Serializable]
    public class AudioEntry
    {
        public string m_audioName;
        public AudioClip m_audioClip;
    }
    
    [Serializable]
    public class AudioLibrary
    {
        public List<AudioEntry> m_audioEntries = new List<AudioEntry>();
    }

    public AudioLibrary m_audioLibrary;

    public void PlayPlayerSFX(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                sfxPlayerSource.pitch = Random.Range(0.85f, 1.15f);
                sfxPlayerSource.volume = Random.Range(0.9f, 1.0f);
                sfxPlayerSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            sfxPlayerSource.pitch = Random.Range(0.85f, 1.15f);
            sfxPlayerSource.volume = Random.Range(0.9f, 1.0f);
            sfxPlayerSource.PlayOneShot(clip);
        }
    }

    public void PlayOtherSFX(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                sfxOtherSource.pitch = Random.Range(0.85f, 1.15f);
                sfxOtherSource.volume = Random.Range(0.9f, 1.0f);
                sfxOtherSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            sfxOtherSource.pitch = Random.Range(0.85f, 1.15f);
            sfxOtherSource.volume = Random.Range(0.9f, 1.0f);
            sfxOtherSource.PlayOneShot(clip);
        }
    }

    public void PlayUI(string audioName, AudioClip clip = null)
    {
        // Find the audio name first
        bool found = false;
        foreach (var entry in m_audioLibrary.m_audioEntries)
        {
            if (entry.m_audioName == audioName)
            {
                found = true;
                uiSource.pitch = Random.Range(0.85f, 1.15f);
                uiSource.volume = Random.Range(0.9f, 1.0f);
                uiSource.PlayOneShot(entry.m_audioClip);
            }
        }
        
        if (!found && clip != null)
        {
            uiSource.pitch = Random.Range(0.85f, 1.15f);
            uiSource.volume = Random.Range(0.9f, 1.0f);
            uiSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}