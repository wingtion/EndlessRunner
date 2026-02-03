using UnityEngine;

public class Coin : MonoBehaviour
{
    public AudioClip collectSound;
    public float rotateSpeed = 100f;
    public int coinValue = 1; // Default value is 1 for regular coins

    [Range(0f, 1f)] public float soundVolume = 1f; // Add this line


    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            if (CoinManager.instance != null)
                CoinManager.instance.AddCoin(coinValue); // Pass the coin value

            gameObject.SetActive(false);
        }
    }
}