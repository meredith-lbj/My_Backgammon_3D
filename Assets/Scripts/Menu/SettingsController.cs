using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System;

public class SettingsController : MonoBehaviour
{
    [SerializeField] AudioMixer soundMixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    void Start()
    {
        float musicVolume = AudioManager.instance.musicSource.volume;
        float sfxVolume = AudioManager.instance.sfxSource.volume;
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
    }

    public void UpMusic() {
        float musicVolume = AudioManager.instance.musicSource.volume;
        if (musicVolume >= 1f) return;

        musicVolume += 0.1f;
        soundMixer.SetFloat("musicVolume", Mathf.Log10(musicVolume) * 20);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        AudioManager.instance.SetMusicVolume(musicVolume);
    }

    public void DownMusic() {
        float musicVolume = AudioManager.instance.musicSource.volume;
        if (musicVolume <= 0f) return;

        musicVolume -= 0.1f;
        soundMixer.SetFloat("musicVolume", Mathf.Log10(musicVolume) * 20);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        AudioManager.instance.SetMusicVolume(musicVolume);
    }

    public void UpSfx() {
        float sfxVolume = AudioManager.instance.sfxSource.volume;
        if (sfxVolume >= 1f) return;

        sfxVolume += 0.1f;
        soundMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        AudioManager.instance.SetSFXVolume(sfxVolume);
    }

    public void DownSfx() {
        float sfxVolume = AudioManager.instance.sfxSource.volume;
        if (sfxVolume <= 0f) return;

        sfxVolume -= 0.1f;
        soundMixer.SetFloat("sfxVolume", Mathf.Log10(sfxVolume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        AudioManager.instance.SetSFXVolume(sfxVolume);
    }

}
