using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool isDeadly = false; // Eðer bu engel direkt öldürücüyse

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (isDeadly)
                {
                    player.Die();
                }
                else
                {
                    player.HitObstacle();
                }
            }

            // Engel efektleri vs. burada
            gameObject.SetActive(false); // veya destroy
        }
    }
}