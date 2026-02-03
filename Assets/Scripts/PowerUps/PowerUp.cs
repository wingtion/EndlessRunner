using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    public AudioClip collectSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    protected bool isCollected = false;

    protected virtual void OnEnable()
    {
        isCollected = false;
    }

    protected virtual void Collect(Transform playerTransform)
    {
        if (isCollected) return;

        // Play sound if assigned
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);

        isCollected = true;
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerCollect(other);
        }
    }

    // Each specific power-up defines its own behavior
    protected abstract void OnPlayerCollect(Collider playerCollider);
}
