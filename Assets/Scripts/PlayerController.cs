using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // singleton instance for player controller
    public static PlayerController instance;

    [Header("Movement Settings")]
    public float forwardSpeed = 6f;
    public float laneChangeSpeed = 10f;
    public float jumpForce = 7f;

    [Header("Lane Settings")]
    public int currentLane = 1; // 0: left, 1: middle, 2: right
    public float laneDistance = 2f; // Distance between lanes
    private float targetXPosition; // Target X position for current lane

    private CharacterController controller;
    private float verticalVelocity;
    private float gravity = 12f;

    [Header("Animation")]
    public Animator animator;
    private bool isSliding = false;
    private float originalHeight;
    private Vector3 originalCenter;

    [Header("Stumble Settings")]
    public int maxHealth = 2;
    public float stumbleDuration = 5f;
    public float stumbleSpeedReduction = 0.5f;
    // Removed stumbleControlReduction since we want same left/right speed

    [Header("Game States")]
    public bool isDead = false;
    public bool isStumbling = false;
    public bool isChangingLane = false;

    private int currentHealth;
    private float originalForwardSpeed;

    [Header("Difficulty Settings")]
    private float elapsedTime = 0f;

    private bool isJumping = false;
    private bool colliderAdjustedForJump = false;

    [Header("Distance Traveled")]
    public float distanceTraveled = 0f;
    private Vector3 lastPosition;

    [Header("Score Settings")]
    public float scoreMultiplier = 1f;
    public float currentScore = 0f;

    [Header("Magnet Settings")]
    private bool isMagnetActive = false;
    private float magnetRange = 0f;
    private float magnetTimer = 0f;

    [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public float shieldRotationSpeed = 100f;
    public List<GameObject> activeShields = new List<GameObject>();
    public bool isShieldActive = false;
    private float shieldTimer = 0f;
    private bool shieldUIVisible = false;

    [Header("Audio")]
    public AudioClip hitObstacleSound;
    [Range(0f, 1f)] public float hitObstacleVolume = 1f;
    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 0.7f;
    public AudioClip laneChangeSound;
    [Range(0f, 1f)] public float laneChangeVolume = 0.5f;
    private AudioSource audioSource;

    // NEW: Falling animation variables
    private bool isFalling = false;
    private float fallDetectionThreshold = -0.5f; // Velocity threshold to detect falling

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        originalHeight = controller.height;
        originalCenter = controller.center;
        originalForwardSpeed = forwardSpeed;
        currentHealth = maxHealth;

        animator.SetBool("isRunning", true);
        animator.applyRootMotion = false;

        lastPosition = transform.position;
        audioSource = GetComponent<AudioSource>();

        // Initialize target position to current position
        targetXPosition = transform.position.x;

        if (MusicManager.instance != null)
        {
            MusicManager.instance.PlayGameMusic();
        }
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIController.instance != null)
            {
                bool isPaused = UIController.instance.pausePanel != null && UIController.instance.pausePanel.activeSelf;
                if (isPaused)
                    UIController.instance.HidePausePanel();
                else
                    UIController.instance.ShowPausePanel();
            }
        }

        // Handle lane switching input - ALLOWED EVEN WHEN STUMBLING
        if (!isDead && !isChangingLane)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }
        }

        // Update forward speed based on time
        if (!isDead && !isStumbling)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < 30f)
                forwardSpeed = 6f;
            else if (elapsedTime < 50f)
                forwardSpeed = 7f;
            else if (elapsedTime < 80f)
                forwardSpeed = 8f;
            else if (elapsedTime < 120f)
                forwardSpeed = 9f;
            else if (elapsedTime < 150f)
                forwardSpeed = 10f;
            else if (elapsedTime < 180f)
                forwardSpeed = 11f;
            else
                forwardSpeed = 11f;

            forwardSpeed = Mathf.Min(forwardSpeed, 11f);
        }

        // Forward constant motion
        Vector3 move = Vector3.forward * forwardSpeed * Time.deltaTime;

        // Smooth lane movement - ALWAYS use normal laneChangeSpeed (even when stumbling)
        float newX = Mathf.Lerp(transform.position.x, targetXPosition, laneChangeSpeed * Time.deltaTime);
        move.x = newX - transform.position.x;

        // Check if we're still changing lanes
        isChangingLane = Mathf.Abs(transform.position.x - targetXPosition) > 0.1f;

        // Handle jumping and gravity - DISABLE JUMPING WHEN STUMBLING
        if (controller.isGrounded && !isStumbling)
        {
            verticalVelocity = -0.1f;

            // NEW: Reset both jumping and falling animations when grounded
            if (isJumping || isFalling)
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
                isJumping = false;
                isFalling = false;

                if (!isSliding)
                {
                    controller.center = originalCenter;
                    colliderAdjustedForJump = false;
                }
            }

            if (!isSliding && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                if (jumpSound != null && audioSource != null)
                    audioSource.PlayOneShot(jumpSound, jumpVolume);

                verticalVelocity = jumpForce;
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false); // Ensure falling is false when starting jump
                isJumping = true;
                isFalling = false;

                controller.center = new Vector3(originalCenter.x, originalCenter.y + 0.3f, originalCenter.z);
                colliderAdjustedForJump = true;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;

            // NEW: Detect falling state
            if (!isFalling && isJumping && verticalVelocity < fallDetectionThreshold)
            {
                isFalling = true;
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
                Debug.Log("Started falling animation");
            }
        }
        move.y = verticalVelocity * Time.deltaTime;

        // Sliding - DISABLE SLIDING WHEN STUMBLING
        if (!isSliding && controller.isGrounded && !isStumbling &&
            (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            StartCoroutine(DoSlide());
        }

        controller.Move(move);

        // Handle magnet
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;
            if (magnetTimer <= 0)
            {
                isMagnetActive = false;
                Debug.Log("Magnet ended.");
            }
            else
            {
                Collider[] nearbyCoins = Physics.OverlapSphere(transform.position, magnetRange);
                foreach (var col in nearbyCoins)
                {
                    if (col.CompareTag("Coin"))
                    {
                        col.transform.position = Vector3.MoveTowards(
                            col.transform.position,
                            transform.position + Vector3.up,
                            10f * Time.deltaTime
                        );
                    }
                }
            }
        }

        // Handle shield
        if (isShieldActive && activeShields.Count > 0)
        {
            shieldTimer -= Time.deltaTime;

            if (UIController.instance != null && shieldUIVisible)
            {
                UIController.instance.UpdateShieldTimer(shieldTimer, shieldTimer + Time.deltaTime);
            }

            foreach (GameObject shield in activeShields)
            {
                if (shield != null)
                {
                    shield.transform.RotateAround(transform.position, Vector3.up, shieldRotationSpeed * Time.deltaTime);
                }
            }

            if (shieldTimer <= 0f)
            {
                isShieldActive = false;
                foreach (GameObject shield in activeShields)
                {
                    if (shield != null)
                        Destroy(shield);
                }
                activeShields.Clear();

                if (UIController.instance != null)
                {
                    UIController.instance.HideShieldUI();
                    shieldUIVisible = false;
                }
            }
        }

        // Track distance and update UI
        distanceTraveled += (transform.position - lastPosition).z;
        lastPosition = transform.position;

        int coins = (CoinManager.instance != null) ? CoinManager.instance.coinCount : 0;
        currentScore = (distanceTraveled * scoreMultiplier) + coins;

        if (UIController.instance != null)
        {
            UIController.instance.UpdateDistanceText(distanceTraveled);
            UIController.instance.UpdateScoreText(currentScore);
        }
    }

    public void MoveLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            targetXPosition = transform.position.x - laneDistance;
            PlayLaneChangeSound();
            Debug.Log($"Moving to left lane: {currentLane}");

            if (isStumbling)
            {
                Debug.Log("Stumbling but moving left at normal speed!");
            }
        }
    }

    public void MoveRight()
    {
        if (currentLane < 2)
        {
            currentLane++;
            targetXPosition = transform.position.x + laneDistance;
            PlayLaneChangeSound();
            Debug.Log($"Moving to right lane: {currentLane}");

            if (isStumbling)
            {
                Debug.Log("Stumbling but moving right at normal speed!");
            }
        }
    }

    private void PlayLaneChangeSound()
    {
        if (laneChangeSound != null && audioSource != null)
        {
            // Use normal sound even when stumbling (since movement is normal)
            audioSource.PlayOneShot(laneChangeSound, laneChangeVolume);
        }
    }

    public void ActivateShield(float duration)
    {
        isShieldActive = true;
        shieldTimer = duration;

        foreach (GameObject shield in activeShields)
        {
            if (shield != null)
                Destroy(shield);
        }
        activeShields.Clear();

        if (shieldPrefab != null)
        {
            float orbitDistance = 1.2f;
            float orbitHeight = 1.2f;

            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f;
                float angleRad = angle * Mathf.Deg2Rad;

                Vector3 orbitOffset = new Vector3(
                    Mathf.Cos(angleRad) * orbitDistance,
                    orbitHeight,
                    Mathf.Sin(angleRad) * orbitDistance
                );

                GameObject shield = Instantiate(shieldPrefab, transform.position + orbitOffset, Quaternion.identity);
                shield.transform.SetParent(transform);
                activeShields.Add(shield);
            }
        }

        if (UIController.instance != null)
        {
            UIController.instance.ShowShieldUI(duration);
            shieldUIVisible = true;
        }

        Debug.Log("3 Shields Activated for " + duration + " seconds!");
    }

    public void ActivateMagnet(float duration, float range)
    {
        magnetTimer = duration;
        magnetRange = range;
        isMagnetActive = true;
        Debug.Log("Magnet Activated for " + duration + " seconds!");
    }

    public void HitObstacle()
    {
        if (isDead || isStumbling) return;

        if (hitObstacleSound != null && audioSource != null)
            audioSource.PlayOneShot(hitObstacleSound, hitObstacleVolume);

        // NEW: Reset falling animation if hit obstacle in air
        if (isFalling)
        {
            animator.SetBool("isFalling", false);
            isFalling = false;
        }

        if (isShieldActive && activeShields.Count > 0)
        {
            GameObject shieldToRemove = activeShields[0];
            activeShields.RemoveAt(0);
            Destroy(shieldToRemove);

            Debug.Log("Shield destroyed! " + activeShields.Count + " shields remaining.");

            if (activeShields.Count == 0)
            {
                isShieldActive = false;
                if (UIController.instance != null)
                {
                    UIController.instance.HideShieldUI();
                    shieldUIVisible = false;
                }
                Debug.Log("All shields destroyed!");
            }
            return;
        }

        currentHealth--;

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(Stumble());
    }

    private IEnumerator Stumble()
    {
        isStumbling = true;

        // NEW: Reset jumping/falling animations when stumbling
        if (isJumping || isFalling)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            isJumping = false;
            isFalling = false;
        }

        // Only reduce forward speed, NOT lane change speed
        forwardSpeed = originalForwardSpeed * stumbleSpeedReduction;

        // Injured animasyonuna geç
        animator.SetBool("isInjured", true);
        Debug.Log("Set isInjured to true - Player moves slower forward but normal left/right");

        // 2.5 saniye Injured Run'da kal
        yield return new WaitForSeconds(2.5f);

        // Fast Run'a geri dön
        animator.SetBool("isInjured", false);
        Debug.Log("Set isInjured to false");

        // Hýzý normale getir
        forwardSpeed = originalForwardSpeed;
        isStumbling = false;

        Debug.Log("Fully returned to Fast Run with full movement control");
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // NEW: Reset all animation states when dying
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);
        animator.SetBool("isInjured", false);
        animator.SetBool("isSliding", false);

        if (MusicManager.instance != null)
            MusicManager.instance.StopGameMusic();

        animator.SetTrigger("Die");
        animator.SetBool("isRunning", false);
        forwardSpeed = 0f;

        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(2.1f);

        if (UIController.instance != null)
        {
            UIController.instance.ShowGameOverPanel();
        }
    }

    private IEnumerator DoSlide()
    {
        isSliding = true;
        animator.SetBool("isSliding", true);
        isJumping = false;
        isFalling = false; // NEW: Reset falling when sliding

        float slideHeightMultiplier = 0.5f;
        controller.height = originalHeight * slideHeightMultiplier;
        controller.center = new Vector3(originalCenter.x, originalCenter.y - originalHeight * 0.25f, originalCenter.z);

        float slideDuration = GetSlideAnimationLength();
        yield return new WaitForSeconds(slideDuration);

        controller.height = originalHeight;
        controller.center = originalCenter;
        colliderAdjustedForJump = false;

        animator.SetBool("isSliding", false);
        isSliding = false;
    }

    private float GetSlideAnimationLength()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.ToLower().Contains("slide"))
            {
                return clip.length;
            }
        }
        Debug.LogWarning("Slide animation not found, using default duration");
        return 1.0f;
    }

    void OnDrawGizmosSelected()
    {
        // Draw lane gizmos centered on player
        Gizmos.color = Color.blue;

        Vector3 playerPos = transform.position;

        // Draw the three lanes relative to player's current position
        for (int i = -1; i <= 1; i++)
        {
            float laneX = playerPos.x + (i * laneDistance);
            Vector3 laneCenter = new Vector3(laneX, playerPos.y, playerPos.z);

            // Draw lane center line
            Gizmos.color = i == 0 ? Color.green : Color.blue; // Middle lane is green
            Gizmos.DrawLine(
                laneCenter + Vector3.forward * 10f,
                laneCenter + Vector3.back * 10f
            );

            // Draw lane boundaries
            Gizmos.color = Color.red;
            float halfLaneWidth = laneDistance / 2f;
            Vector3 leftBound = new Vector3(laneX - halfLaneWidth, playerPos.y, playerPos.z);
            Vector3 rightBound = new Vector3(laneX + halfLaneWidth, playerPos.y, playerPos.z);

            Gizmos.DrawLine(
                leftBound + Vector3.forward * 10f,
                leftBound + Vector3.back * 10f
            );
            Gizmos.DrawLine(
                rightBound + Vector3.forward * 10f,
                rightBound + Vector3.back * 10f
            );
        }

        // Highlight current lane
        Gizmos.color = Color.yellow;
        float currentLaneX = playerPos.x + ((currentLane - 1) * laneDistance);
        Vector3 currentLanePos = new Vector3(currentLaneX, playerPos.y, playerPos.z);
        Gizmos.DrawWireCube(currentLanePos, new Vector3(0.3f, 2f, 1f));

        // Optional: Draw different color when stumbling
        if (isStumbling)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}