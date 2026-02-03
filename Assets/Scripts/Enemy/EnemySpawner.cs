using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int poolSize = 10;
    public float spawnIntervalZ = 15f;   // Distance along Z to spawn new enemy
    public float xSpawnRange = 3f;       // Random horizontal offset
    public float spawnYOffset = 0.5f;

    [Header("References")]
    public Transform player;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private float nextSpawnZ = 0f;

    void Start()
    {
        if (player == null)
            player = PlayerController.instance?.transform;

        // Create object pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Enqueue(enemy);
        }

        nextSpawnZ = player.position.z + 50f; // first spawn ahead
    }

    void Update()
    {
        if (player == null) return;

        // Spawn enemies ahead of player
        if (player.position.z + 70f > nextSpawnZ)
        {
            SpawnEnemy(nextSpawnZ);
            nextSpawnZ += spawnIntervalZ;
        }
    }

    void SpawnEnemy(float zPos)
    {
        if (enemyPool.Count == 0) return;

        GameObject enemy = enemyPool.Dequeue();
        Vector3 spawnPos = new Vector3(
    player.position.x + Random.Range(-xSpawnRange, xSpawnRange),
    player.position.y + spawnYOffset,
    zPos
);

        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;
        enemy.SetActive(true);

        // Return to pool after a delay (auto disable handled in EnemyAI)
        StartCoroutine(RepoolAfterDelay(enemy, 10f));
    }

    private IEnumerator RepoolAfterDelay(GameObject enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (enemy != null && !enemy.activeInHierarchy)
        {
            enemyPool.Enqueue(enemy);
        }
    }
}
