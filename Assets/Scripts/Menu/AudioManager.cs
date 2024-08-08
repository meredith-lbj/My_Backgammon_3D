using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioMixer soundMixer;
    [SerializeField] AudioClip[] musicSounds;
    [SerializeField] AudioClip[] sfxSounds;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudio();
        } else {
            Destroy(gameObject);
        }
    }
    void Start()
    {
    }

    void InitAudio()
    {
        if (PlayerPrefs.HasKey("musicVolume")) {
            SetMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
        } else {
            SetMusicVolume(0.5f);
        }
        soundMixer.SetFloat("musicVolume", Mathf.Log10(musicSource.volume) * 20);
        if (PlayerPrefs.HasKey("sfxVolume")) {
            SetSFXVolume(PlayerPrefs.GetFloat("sfxVolume"));
        } else {
            SetSFXVolume(0.5f);
        }
        soundMixer.SetFloat("sfxVolume", Mathf.Log10(sfxSource.volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", musicSource.volume);
        PlayerPrefs.SetFloat("sfxVolume", sfxSource.volume);
        if (musicSounds.Length > 0) {
            playMusicByName(musicSounds[0].name);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    public void pauseMusic()
    {
        musicSource.Pause();
    }

    public void playMusic()
    {
        musicSource.Play();
    }

    public void playMusicByName(string name)
    {
        AudioClip clip = Array.Find(musicSounds, x => x.name == name);

        if (clip == null) return;
        else {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void playSFXByName(string name)
    {
        if (sfxSource.volume <= 0f) return;

        AudioClip clip = Array.Find(sfxSounds, x => x.name == name);

        if (clip == null) return;
        else {
            sfxSource.clip = clip;
            sfxSource.PlayOneShot(clip);
        }
    }
}
