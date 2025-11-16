using UnityEngine;

public class FallingObstaclePlatform : MonoBehaviour
{
    [SerializeField] private GameObject fallingObstaclePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool hasSpawnedObstacles = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasSpawnedObstacles) return;
        int pointsToActivate = Random.Range(1, spawnPoints.Length + 1);
        System.Collections.Generic.List<int> selectedIndices = new System.Collections.Generic.List<int>();
        while (selectedIndices.Count < pointsToActivate)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Instantiate(fallingObstaclePrefab, spawnPoints[randomIndex].position, Quaternion.identity, transform);
            }
        }
        hasSpawnedObstacles = true;
    }

    private void OnDestroy()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
