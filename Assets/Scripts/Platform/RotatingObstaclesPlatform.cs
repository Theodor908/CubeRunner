using UnityEngine;

public class RotatingObstaclesPlatform : BasePlatform
{
    [SerializeField] private GameObject rotatingObstaclePrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        int pointsToActivate = Random.Range(1, spawnPoints.Length + 1);
        System.Collections.Generic.List<int> selectedIndices = new System.Collections.Generic.List<int>();
        while (selectedIndices.Count < pointsToActivate)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Instantiate(rotatingObstaclePrefab, spawnPoints[randomIndex].position, Quaternion.identity, transform);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
