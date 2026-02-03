using System.Collections.Generic;
using UnityEngine;

public class OptimizedSegmentGenerator : MonoBehaviour
{
    [Header("Segment Settings")]
    public GameObject[] segmentPrefabs;
    public int poolSizePerSegment = 3;
    public float segmentLength = 50f;
    public int maxActiveSegments = 6;

    [Header("Player Reference")]
    public Transform player;
    public float spawnTriggerDistance = 70f;

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private List<GameObject> pooledSegments = new List<GameObject>();
    private float nextSpawnZ = 0f;
    private int lastSpawnedIndex = -1;

    void Start()
    {
        // Pre-create pooled segments
        foreach (var prefab in segmentPrefabs)
        {
            for (int i = 0; i < poolSizePerSegment; i++)
            {
                GameObject seg = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                seg.SetActive(false);
                pooledSegments.Add(seg);
            }
        }

        // Spawn initial random segments
        for (int i = 0; i < maxActiveSegments; i++)
        {
            SpawnNextSegment();
        }
    }

    void Update()
    {
        if (player == null || activeSegments.Count == 0) return;

        float distanceToLast = nextSpawnZ - player.position.z;
        if (distanceToLast < spawnTriggerDistance)
        {
            SpawnNextSegment();
            RemoveOldestSegment();
        }
    }

    void SpawnNextSegment()
    {
        int randomIndex = GetRandomDifferentIndex();
        GameObject chosenPrefab = segmentPrefabs[randomIndex];

        GameObject segment = GetPooledSegment(chosenPrefab);
        if (segment == null) return;

        segment.transform.position = new Vector3(0, 0, nextSpawnZ);
        segment.SetActive(true);

        activeSegments.Enqueue(segment);
        nextSpawnZ += segmentLength;

        lastSpawnedIndex = randomIndex;
    }

    int GetRandomDifferentIndex()
    {
        if (segmentPrefabs.Length <= 1) return 0;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, segmentPrefabs.Length);
        }
        while (randomIndex == lastSpawnedIndex);

        return randomIndex;
    }

    GameObject GetPooledSegment(GameObject prefab)
    {
        foreach (var seg in pooledSegments)
        {
            if (!seg.activeInHierarchy && seg.name.StartsWith(prefab.name))
                return seg;
        }
        return null;
    }

    void RemoveOldestSegment()
    {
        if (activeSegments.Count > maxActiveSegments)
        {
            GameObject old = activeSegments.Dequeue();
            old.SetActive(false);
        }
    }
}
