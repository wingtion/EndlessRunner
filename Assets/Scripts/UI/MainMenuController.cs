using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;
    public Slider gameMusicSlider;
    public Slider menuMusicSlider;

    void Start()
    {
        // Initialize sliders directly without triggering events
        InitializeSlidersSafe();

        // Hide panels at start
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    private void InitializeSlidersSafe()
    {
        // Set slider values directly without listeners first
        if (gameMusicSlider != null && MusicManager.instance != null)
        {
            gameMusicSlider.SetValueWithoutNotify(MusicManager.instance.GetGameVolume());
            gameMusicSlider.onValueChanged.AddListener(OnGameMusicVolumeChanged);
        }

        if (menuMusicSlider != null && MusicManager.instance != null)
        {
            menuMusicSlider.SetValueWithoutNotify(MusicManager.instance.GetMenuVolume());
            menuMusicSlider.onValueChanged.AddListener(OnMenuMusicVolumeChanged);
        }
    }

    public void OnPlayButton()
    {
        // Switch to game music with fade
        if (MusicManager.instance != null)
            MusicManager.instance.PlayGameMusic();

        // Load your gameplay scene
        SceneManager.LoadScene("Game");
    }

    public void OnQuitButton()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void OnOptionsButton()
    {
        // Show settings panel and hide how to play panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            // Update sliders when panel is shown
            if (gameMusicSlider != null && MusicManager.instance != null)
                gameMusicSlider.SetValueWithoutNotify(MusicManager.instance.GetGameVolume());

            if (menuMusicSlider != null && MusicManager.instance != null)
                menuMusicSlider.SetValueWithoutNotify(MusicManager.instance.GetMenuVolume());
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    public void OnCloseSettingsButton()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnHowToPlayButton()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnCloseHowToPlayButton()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // Volume change methods - these only get called by user interaction
    public void OnGameMusicVolumeChanged(float value)
    {
        if (MusicManager.instance != null)
            MusicManager.instance.SetGameVolume(value);
    }

    public void OnMenuMusicVolumeChanged(float value)
    {
        if (MusicManager.instance != null)
            MusicManager.instance.SetMenuVolume(value);
    }

    public void OnCreditsButton()
    {
        Debug.Log("Show Credits");
    }
}