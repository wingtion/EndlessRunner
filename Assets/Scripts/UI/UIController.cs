using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public TMP_Text coinText;
    public TMP_Text distanceText;
    public TMP_Text scoreText;

    [Header("Game Over Panel")]
    // Game Over Panel references
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public TMP_Text finalCoinText;
    public TMP_Text finalDistanceText;
    public TMP_Text bestScoreText;
    public TMP_Text recordMessageText;

    [Header("Pause Menu")]
    public GameObject pausePanel;

    [Header("Tips")]
    public TMP_Text tipText;

    private string[] tips = new string[]
    {
        "Tip: Slide under low obstacles!",
        "Tip: Jump early to clear long gaps.",
        "Tip: Collect magnets to grab distant coins!",
        "Tip: Shields can save you from 3 hits.",
        "Tip: The longer you run, the faster it gets!"
    };

    [Header("Particle Effects")]
    public ParticleSystem PRParticleEffectPrefab;
    private ParticleSystem prEffectInstance;


    [Header("Audio")]
    public AudioClip newRecordSound; // Add this field for the new record sound
    [Range(0f, 1f)]
    public float newRecordVolume = 1f;
    private AudioSource audioSource;

    private Coroutine prPulseCoroutine;
    private Coroutine prBounceCoroutine;


    [Header("Shield UI")]
    public GameObject shieldUIPanel; // Parent object for shield UI
    public Image shieldIcon;
    public Slider shieldTimerSlider;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Ensure game over panel is hidden at start
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Instantiate particle effect as a child of the Canvas or this UI controller
        // NOT as a child of the inactive gameOverPanel
        if (PRParticleEffectPrefab != null)
        {
            // Instantiate as child of this object or canvas
            prEffectInstance = Instantiate(PRParticleEffectPrefab, transform);
            prEffectInstance.transform.localPosition = Vector3.zero;
            prEffectInstance.gameObject.SetActive(false); // hide initially

            // Make sure it's on top by setting a high Z position or using Sort Order
            Vector3 pos = prEffectInstance.transform.localPosition;
            pos.z = -10f; // Adjust this value as needed
            prEffectInstance.transform.localPosition = pos;
        }
    }

    public void ShowShieldUI(float duration)
    {
        if (shieldUIPanel != null)
            shieldUIPanel.SetActive(true);

        if (shieldTimerSlider != null)
        {
            shieldTimerSlider.maxValue = duration;
            shieldTimerSlider.value = duration;
        }
    }

    public void UpdateShieldTimer(float timeLeft, float totalDuration)
    {
        if (shieldTimerSlider != null)
        {
            shieldTimerSlider.value = timeLeft;
        }
    }

    public void HideShieldUI()
    {
        if (shieldUIPanel != null)
            shieldUIPanel.SetActive(false);
    }

    //method to change the record message text's color repeatedly
    private IEnumerator PulseRecordMessage()
    {
        Color color1 = Color.yellow;
        Color color2 = Color.white;
        float speed = 2f; // how fast it pulses

        while (true)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            recordMessageText.color = Color.Lerp(color1, color2, t);
            yield return null;
        }
    }

    private IEnumerator BounceRecordMessage()
    {
        Vector3 originalScale = recordMessageText.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        float duration = 0.5f;

        while (true)
        {
            // scale up
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                recordMessageText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t / duration);
                yield return null;
            }

            // scale down
            t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                recordMessageText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t / duration);
                yield return null;
            }
        }
    }



    public void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f; // Freeze the game

        // Pause in-game music
        if (MusicManager.instance != null && MusicManager.instance.gameMusicSource != null)
            MusicManager.instance.gameMusicSource.Pause();

        // Show a random tip when paused
        if (tipText != null && tips.Length > 0)
        {
            int randomIndex = Random.Range(0, tips.Length);
            tipText.text = tips[randomIndex];
        }
    }

    public void HidePausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f; // Resume the game

        // Resume in-game music
        if (MusicManager.instance != null && MusicManager.instance.gameMusicSource != null)
            MusicManager.instance.gameMusicSource.UnPause();
    }

    public void OnResumeButton()
    {
        HidePausePanel();
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;

        if (MusicManager.instance != null)
            MusicManager.instance.PlayMenuMusic();

        SceneManager.LoadScene("MainMenu");
    }

    public void ShowGameOverPanel()
    {
        // Calculate final stats
        if (finalScoreText != null && PlayerController.instance != null)
            finalScoreText.text = "Score: " + Mathf.FloorToInt(PlayerController.instance.currentScore).ToString();

        if (finalCoinText != null && CoinManager.instance != null)
            finalCoinText.text = "Coins: " + CoinManager.instance.coinCount.ToString();

        if (finalDistanceText != null && PlayerController.instance != null)
            finalDistanceText.text = "Distance: " + Mathf.FloorToInt(PlayerController.instance.distanceTraveled).ToString() + "m";

        // --- Handle Personal Record (Best Score) ---
        bool isNewRecord = false;
        if (bestScoreText != null && PlayerController.instance != null)
        {
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            int currentScore = Mathf.FloorToInt(PlayerController.instance.currentScore);

            if (currentScore > bestScore)
            {
                PlayerPrefs.SetInt("BestScore", currentScore);
                bestScoreText.text = $"PR: {currentScore}";

                if (recordMessageText != null)
                {
                    recordMessageText.text = "NEW PERSONAL RECORD!!!";

                    // Stop previous coroutines first
                    if (prPulseCoroutine != null) StopCoroutine(prPulseCoroutine);
                    if (prBounceCoroutine != null) StopCoroutine(prBounceCoroutine);

                    // Start new coroutines
                    prPulseCoroutine = StartCoroutine(PulseRecordMessage());
                    prBounceCoroutine = StartCoroutine(BounceRecordMessage());

                }

                // PLAY NEW RECORD SOUND
                PlayNewRecordSound();

                isNewRecord = true;
            }
            else
            {
                bestScoreText.text = $"PR: {bestScore}";
                if (recordMessageText != null)
                {
                    recordMessageText.text = "";
                    recordMessageText.gameObject.SetActive(false);
                }
            }
        }

        // Show tip
        if (tipText != null && tips.Length > 0)
        {
            int randomIndex = Random.Range(0, tips.Length);
            tipText.text = tips[randomIndex];
        }

        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Handle particle effect AFTER the panel is active
        if (prEffectInstance != null)
        {
            if (isNewRecord)
            {
                // Position the particle effect relative to the game over panel
                if (gameOverPanel != null)
                {
                    // Get the center of the game over panel in screen space
                    RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
                    if (panelRect != null)
                    {
                        Vector3 panelCenter = panelRect.position;
                        prEffectInstance.transform.position = panelCenter;
                    }
                }

                prEffectInstance.gameObject.SetActive(true);
                prEffectInstance.Play();
                Debug.Log("Playing PR particle effect!");
            }
            else
            {
                prEffectInstance.gameObject.SetActive(false);
                prEffectInstance.Stop();
            }
        }
    }

    // Add this method to play the new record sound
    private void PlayNewRecordSound()
    {
        if (newRecordSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(newRecordSound, newRecordVolume);
            Debug.Log("Playing new record sound!");
        }
        else
        {
            Debug.LogWarning("New record sound or AudioSource is missing!");
        }
    }


    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Stop particle effect when hiding panel
        if (prEffectInstance != null)
        {
            prEffectInstance.Stop();
            prEffectInstance.gameObject.SetActive(false);
        }

        //stop the coroutines for making the record message text look alive
        if (prPulseCoroutine != null) StopCoroutine(prPulseCoroutine);
        if (prBounceCoroutine != null) StopCoroutine(prBounceCoroutine);
        recordMessageText.color = Color.white;
        recordMessageText.transform.localScale = Vector3.one; // reset scale from bounce
        recordMessageText.gameObject.SetActive(false);
    }

    public void OnTryAgainButton()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // NEW METHOD: Reset Personal Record
    public void OnResetRecordButton()
    {
        // Reset the best score in PlayerPrefs
        PlayerPrefs.DeleteKey("BestScore");
        PlayerPrefs.Save();

        // Update the UI to show PR as 0
        if (bestScoreText != null)
            bestScoreText.text = "PR: 0";

        //stop the coroutines for making the record message text look alive
        if (prPulseCoroutine != null) StopCoroutine(prPulseCoroutine);
        if (prBounceCoroutine != null) StopCoroutine(prBounceCoroutine);
        recordMessageText.color = Color.white;
        recordMessageText.transform.localScale = Vector3.one;
        recordMessageText.text = "";
        recordMessageText.gameObject.SetActive(false);



        // Stop and hide the particle effect if it's playing
        if (prEffectInstance != null)
        {
            prEffectInstance.Stop();
            prEffectInstance.gameObject.SetActive(false);
        }

        Debug.Log("Personal record reset to 0!");
    }

    public void UpdateCoinText(int coinCount)
    {
        if (coinText != null)
            coinText.text = coinCount.ToString();
    }

    public void UpdateDistanceText(float distance)
    {
        if (distanceText != null)
            distanceText.text = Mathf.FloorToInt(distance).ToString() + "m";
    }

    public void UpdateScoreText(float score)
    {
        if (scoreText != null)
            scoreText.text = Mathf.FloorToInt(score).ToString();
    }
}