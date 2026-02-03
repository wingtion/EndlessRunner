using UnityEngine;
using System.Collections.Generic;

public class SegmentResetter : MonoBehaviour
{
    private Dictionary<Transform, Vector3> originalLocalPositions = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        // Store the original local positions of all coins & power-ups once
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("Coin") || child.CompareTag("PowerUp"))
                originalLocalPositions[child] = child.localPosition;
        }
    }

    void OnEnable()
    {
        int resetCount = 0;
        foreach (var kvp in originalLocalPositions)
        {
            Transform collectible = kvp.Key;
            if (collectible == null) continue;

            // Reset active state and position
            collectible.localPosition = kvp.Value;
            collectible.gameObject.SetActive(true);
            resetCount++;
        }

        Debug.Log($"{name} reset {resetCount} collectibles and restored their positions.");
    }
}
