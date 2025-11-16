using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StandingPlatform : BasePlatform
{
    [SerializeField] private GameObject rotatingObstacle;
    [SerializeField] private List<GameObject> spawnPoints;

    private void Awake()
    {
        int pointsToActivate = Random.Range(1, spawnPoints.Count + 1);
        List<int> selectedIndices = new List<int>();

        while (selectedIndices.Count < pointsToActivate)
        {
            int randomIndex = Random.Range(0, spawnPoints.Count);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Instantiate(rotatingObstacle, spawnPoints[randomIndex].transform.position, Quaternion.identity, transform);
            }
        }
    }

}
