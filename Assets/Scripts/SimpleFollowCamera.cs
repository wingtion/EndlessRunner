using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 5f;

    [Header("Look Settings")]
    public float lookHeight = 1f;

    void Start()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
            transform.LookAt(player.position + Vector3.up * lookHeight);
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Follow player (X and Z only, maintain camera height)
        Vector3 targetPosition = player.position + offset;
        targetPosition.y = transform.position.y;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Look at player (slightly ahead for better view)
        Vector3 lookTarget = player.position + player.forward * 3f + Vector3.up * lookHeight;
        transform.LookAt(lookTarget);
    }
}