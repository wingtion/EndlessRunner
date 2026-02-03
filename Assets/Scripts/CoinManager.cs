using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;
    public int coinCount = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void AddCoin(int value = 1) // Default value is 1 for backward compatibility
    {
        coinCount += value;

        // Update the coin count in the UI
        if (UIController.instance != null)
            UIController.instance.UpdateCoinText(coinCount);
    }

    public void ResetCoins()
    {
        coinCount = 0;

        if (UIController.instance != null)
            UIController.instance.UpdateCoinText(coinCount);
    }
}