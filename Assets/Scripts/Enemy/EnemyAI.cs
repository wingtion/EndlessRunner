using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float chaseRange = 30f;
    public float deactivateAfter = 6f;
    public float turnSmoothness = 2f; // how smoothly it turns toward player
    public float aimError = 2f;       // randomness in direction (higher = less accurate)
    public float stopBehindDistance = 2f; // how far behind player before deactivating

    [Header("Audio Settings")]
    public AudioClip chaseStartClip;    // sound when enemy first detects player
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private AudioSource audioSource;

    private Transform player;
    private bool isChasing = false;
    private bool hasPlayedChaseSound = false;
    private float timer;
    private Vector3 moveDirection;

    void Start()
    {
        player = PlayerController.instance?.transform;
        timer = deactivateAfter;
        moveDirection = transform.forward;

        // Ensure there’s an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // make it 3D sound
        audioSource.volume = sfxVolume;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Start chasing only if within range
        if (!isChasing && distance < chaseRange)
        {
            isChasing = true;

            //  Play chase start sound once
            if (!hasPlayedChaseSound && chaseStartClip != null)
            {
                audioSource.PlayOneShot(chaseStartClip, sfxVolume);
                hasPlayedChaseSound = true;
            }
        }

        // Stop chasing if the enemy has passed behind the player
        if (transform.position.z < player.position.z - stopBehindDistance)
        {
            gameObject.SetActive(false);
            return;
        }

        if (isChasing)
        {
            // Slightly imperfect aim
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            dirToPlayer += new Vector3(
                Random.Range(-aimError, aimError) * 0.05f,
                0,
                Random.Range(-aimError, aimError) * 0.05f
            );

            // Smoothly turn toward that direction
            moveDirection = Vector3.Lerp(moveDirection, dirToPlayer, Time.deltaTime * turnSmoothness);

            // Move forward
            transform.position += moveDirection.normalized * speed * Time.deltaTime;

            // Face the direction of movement
            if (moveDirection.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        // Auto deactivate after a few seconds
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.HitObstacle();
            }

            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        timer = deactivateAfter;
        isChasing = false;
        hasPlayedChaseSound = false; // reset sound for next activation
        moveDirection = transform.forward;
    }
}
