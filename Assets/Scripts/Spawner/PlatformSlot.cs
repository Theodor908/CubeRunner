using System.Collections.Generic;
using UnityEngine;

public class PlatformSlot : MonoBehaviour
{
    private int slotIndex;
    private GameObject currentPlatform;
    private List<GameObject> platformPrefabs;
    private Transform parentTransform;
    private float spawnTime;
    private Vector3 platformPosition;

    public PlatformSlot(int index, List<GameObject> prefabs, Transform parent)
    {
        slotIndex = index;
        platformPrefabs = prefabs;
        parentTransform = parent;
        currentPlatform = null;
        spawnTime = 0f;
    }

    public void SpawnPlatform(Vector3 position)
    {
        if (currentPlatform != null)
        {
            Object.Destroy(currentPlatform);
        }

        GameObject prefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];

        currentPlatform = Object.Instantiate(prefab, position, Quaternion.identity, parentTransform);
        currentPlatform.name = $"Platform_Slot{slotIndex}";

        platformPosition = position;
        spawnTime = Time.time;

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            currentPlatform.layer = groundLayer;
        }
    }

    public void DestroyPlatform()
    {
        if (currentPlatform != null)
        {
            Object.Destroy(currentPlatform);
            currentPlatform = null;
        }
    }

    public bool IsActive()
    {
        return currentPlatform != null;
    }

    public float GetPlatformZ()
    {
        if (currentPlatform != null)
        {
            return currentPlatform.transform.position.z;
        }
        return platformPosition.z;
    }

    public GameObject GetPlatform()
    {
        return currentPlatform;
    }

    public float GetTimeSinceSpawn()
    {
        return Time.time - spawnTime;
    }

    public int GetSlotIndex()
    {
        return slotIndex;
    }
}
