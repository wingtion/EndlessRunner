using UnityEngine;

public class ShieldPowerUp : PowerUp
{
    public float shieldDuration = 5f;

    protected override void OnPlayerCollect(Collider playerCollider)
    {
        PlayerController player = playerCollider.GetComponent<PlayerController>();
        if (player != null)
        {
            player.ActivateShield(shieldDuration);
        }

        Collect(playerCollider.transform);
        Debug.Log("Shield collected!");
    }
}
