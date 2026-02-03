using UnityEngine;

public class MagnetPowerUp : PowerUp
{
    public float duration = 5f;
    public float radius = 10f;

    protected override void OnPlayerCollect(Collider playerCollider)
    {
        PlayerController player = playerCollider.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ActivateMagnet(duration, radius);
        }

        Collect(playerCollider.transform);
        Debug.Log("Magnet collected!");
    }
}
