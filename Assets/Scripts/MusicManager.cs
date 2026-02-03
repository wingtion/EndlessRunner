using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Audio Sources")]
    public AudioSource menuMusicSource;
    public AudioSource gameMusicSource;

    [Header("Settings")]
    [Range(0f, 1f)] public float menuVolume = 1f;
    [Range(0f, 1f)] public float gameVolume = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved volume settings FIRST
            LoadVolumeSettings();

            // Apply volumes immediately after loading
            ApplyVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Start with menu music
        if (menuMusicSource != null)
            menuMusicSource.Play();

        if (gameMusicSource != null)
            gameMusicSource.volume = 0f;
    }

    private void Update()
    {
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (menuMusicSource != null)
            menuMusicSource.volume = menuVolume;

        if (gameMusicSource != null)
            gameMusicSource.volume = gameVolume;
    }

    private void LoadVolumeSettings()
    {
        // Load saved volume settings or use defaults - FIXED DEFAULTS
        menuVolume = PlayerPrefs.GetFloat("MenuVolume", 0.5f);
        gameVolume = PlayerPrefs.GetFloat("GameVolume", 0.5f);

        Debug.Log($"Loaded volumes - Menu: {menuVolume}, Game: {gameVolume}");
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MenuVolume", menuVolume);
        PlayerPrefs.SetFloat("GameVolume", gameVolume);
        PlayerPrefs.Save();

        Debug.Log($"Saved volumes - Menu: {menuVolume}, Game: {gameVolume}");
    }

    // Add this method to get current volumes for UI
    public float GetMenuVolume()
    {
        return menuVolume;
    }

    public float GetGameVolume()
    {
        return gameVolume;
    }

    public void PlayGameMusic()
    {
        if (menuMusicSource != null)
            menuMusicSource.Stop();

        if (gameMusicSource != null)
        {
            gameMusicSource.volume = gameVolume;
            gameMusicSource.Play();
        }
    }

    public void StopGameMusic()
    {
        if (gameMusicSource != null)
            gameMusicSource.Stop();
    }

    public void PlayMenuMusic()
    {
        if (gameMusicSource != null)
            gameMusicSource.Stop();

        if (menuMusicSource != null)
        {
            menuMusicSource.volume = menuVolume;
            menuMusicSource.Play();
        }
    }

    // Public methods for UI sliders
    public void SetMenuVolume(float value)
    {
        menuVolume = value;
        if (menuMusicSource != null)
            menuMusicSource.volume = value;

        // Save the setting
        SaveVolumeSettings();
    }

    public void SetGameVolume(float value)
    {
        gameVolume = value;
        if (gameMusicSource != null)
            gameMusicSource.volume = value;

        // Save the setting
        SaveVolumeSettings();
    }
}