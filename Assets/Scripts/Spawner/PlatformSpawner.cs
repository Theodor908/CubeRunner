using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlatformSpawner : MonoBehaviour
{

    [Header("Platform Prefabs")]
    [SerializeField] private List<GameObject> platformPrefabs;
    [SerializeField] private GameObject defaultPlatformPrefab;
    private GameObject previousPlatform;

    [Header("Spawn Control")]
    [SerializeField] private int platformsPerBatch = 6;
    [SerializeField] private float spawnTriggerDistance = 30f;
    [SerializeField] private int maxActivePlatforms = 10;

    [Header("Platform Spacing (Speed-Based)")]
    [SerializeField] private float speedGapMultiplier = 0.3f;
    [SerializeField] private float minGap = 3f;
    [SerializeField] private float maxGap = 15f;
    [SerializeField] private float gapRandomness = 2f;

    [Header("Intelligent Positioning")]
    [Tooltip("How much to follow player's horizontal position (0 = random, 1 = exact)")]
    [SerializeField] private float horizontalFollowStrength = 0.6f;
    [Tooltip("Random variation added to horizontal position")]
    [SerializeField] private float horizontalRandomness = 2f;
    [Tooltip("Maximum horizontal change between platforms")]
    [SerializeField] private float maxHorizontalDelta = 3f;

    [Tooltip("How much to follow player's height (0 = random, 1 = exact)")]
    [SerializeField] private float verticalFollowStrength = 0.5f;
    [Tooltip("Random variation added to vertical position")]
    [SerializeField] private float verticalRandomness = 1f;
    [Tooltip("Maximum vertical change between platforms")]
    [SerializeField] private float maxVerticalDelta = 2f;

    [Header("Height Constraints")]
    [SerializeField] private float minAbsoluteY = -5f;
    [SerializeField] private float maxAbsoluteY = 2f;

    [Header("Horizontal Constraints")]
    [SerializeField] private float minAbsoluteX = -8f;
    [SerializeField] private float maxAbsoluteX = 8f;

    [Header("Cleanup")]
    [SerializeField] private float cleanupDistance = 25f;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CubePlayerController playerController;

    // Internal state
    private List<PlatformData> activePlatforms;
    private Vector3 lastPlatformPosition;
    private bool isSpawning = false;

    private void Awake()
    {
        // Find player
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerController = player.GetComponent<CubePlayerController>();
            }
        }

        // Validate prefabs
        if (platformPrefabs == null || platformPrefabs.Count == 0)
        {
            if (defaultPlatformPrefab != null)
                platformPrefabs = new List<GameObject> { defaultPlatformPrefab };
            else
                Debug.LogError("No platform prefabs assigned!");
        }

        activePlatforms = new List<PlatformData>();
    }

    private void Start()
    {
        SpawnInitialPlatform();
        SpawnBatch();
    }

    private void Update()
    {
        if (player == null) return;

        if (ShouldSpawnNextBatch())
        {
            SpawnBatch();
        }

        CleanupOldPlatforms();
    }

    #region Initial Setup

    private void SpawnInitialPlatform()
    {
        // Spawn starting platform under player
        Vector3 startPos = player.position;
        startPos.y -= 2f; // Slightly below player
        GameObject prefab = defaultPlatformPrefab;
        previousPlatform = prefab;
        SpawnPlatform(startPos, prefab);
        lastPlatformPosition = startPos;

    }

    #endregion

    #region Batch Spawning

    private bool ShouldSpawnNextBatch()
    {
        if (isSpawning) return false;
        if (activePlatforms.Count >= maxActivePlatforms) return false;

        float distanceToLastPlatform = lastPlatformPosition.z - player.position.z;
        return distanceToLastPlatform < spawnTriggerDistance;
    }

    private void SpawnBatch()
    {
        if (isSpawning) return;

        isSpawning = true;

        int platformsToSpawn = Mathf.Min(platformsPerBatch, maxActivePlatforms - activePlatforms.Count);

        for (int i = 0; i < platformsToSpawn; i++)
        {
            SpawnNextPlatform();
        }

        GameManager.Instance.SetLastPlatformY(lastPlatformPosition.y);
        isSpawning = false;

    }

    private void SpawnNextPlatform()
    {
        GameObject prefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];
        
        while(prefab == previousPlatform && platformPrefabs.Count > 1)
        {
            prefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];
        }

        if (!prefab.TryGetComponent<BasePlatform>(out var basePlatform)) return;

        float platformWidth = basePlatform.BasePlatformWidth;
        float platformHeight = basePlatform.BasePlatformHeight;
        float platformLength = basePlatform.BasePlatformLength;

        float gap = CalculateGapFromSpeed();

        if (gap < 0) gap = 0;

        float spawnZ = lastPlatformPosition.z + gap + platformLength;

        float spawnX = CalculateIntelligentX() + Random.Range(-platformWidth, platformWidth);

        float spawnY = CalculateIntelligentY() - 2 * platformHeight;

        Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);

        SpawnPlatform(spawnPos, prefab);

        lastPlatformPosition = spawnPos;

    }

    private float CalculateGapFromSpeed()
    {
        float playerSpeed = 0f;

        if (playerController != null)
        {
            playerSpeed = playerController.CurrentSpeed;
        }

        float gap = minGap + (playerSpeed * speedGapMultiplier) + Random.Range(0, gapRandomness);


        minGap = minGap + (playerController.CurrentSpeed * speedGapMultiplier); 
        maxGap = maxGap + (playerController.CurrentSpeed * speedGapMultiplier) + playerController.Acceleration;
        gapRandomness = gapRandomness + (playerController.Acceleration * speedGapMultiplier);
        return Mathf.Clamp(gap, minGap, maxGap);
    }

    private float CalculateIntelligentX()
    {
        float playerX = player.position.x;

        float lastX = lastPlatformPosition.x;

        float targetX = Mathf.Lerp(lastX, playerX, horizontalFollowStrength);

        float randomOffset = Random.Range(-horizontalRandomness, horizontalRandomness);
        targetX += randomOffset;

        float deltaX = targetX - lastX;
        deltaX = Mathf.Clamp(deltaX, -maxHorizontalDelta, maxHorizontalDelta);
        targetX = lastX + deltaX;

        targetX = Mathf.Clamp(targetX, minAbsoluteX, maxAbsoluteX);

        return targetX;
    }

    private float CalculateIntelligentY()
    {
        float playerY = player.position.y;

        float lastY = lastPlatformPosition.y;

        float targetY = Mathf.Lerp(lastY, playerY, verticalFollowStrength);

        float randomOffset = Random.Range(-verticalRandomness, 0);
        targetY += randomOffset;

        float deltaY = targetY - lastY;

        if(deltaY > 0)
        {
            deltaY = -deltaY;
        }

        deltaY = Mathf.Clamp(deltaY, minAbsoluteY, maxAbsoluteY);

        targetY = lastY + deltaY;
        targetY -= 0.05f;

        return targetY;
    }

    #endregion

    #region Platform Management

    private void SpawnPlatform(Vector3 position, GameObject prefab)
    {
        GameObject platform = Instantiate(prefab, position, Quaternion.identity, transform);

        PlatformData data = new()
        {
            gameObject = platform,
            position = position,
            spawnTime = Time.time
        };

        activePlatforms.Add(data);
    }

    private void CleanupOldPlatforms()
    {
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            PlatformData platform = activePlatforms[i];

            if (platform.position.z < player.position.z - cleanupDistance)
            {
                Destroy(platform.gameObject);
                activePlatforms.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Public API

    public void Reset()
    {
        foreach (var platform in activePlatforms)
        {
            if (platform.gameObject != null)
                Destroy(platform.gameObject);
        }

        activePlatforms.Clear();
        SpawnInitialPlatform();
        SpawnBatch();
    }

    public int GetActivePlatformCount()
    {
        return activePlatforms.Count;
    }

    public Vector3 GetLastPlatformPosition()
    {
        return lastPlatformPosition;
    }

    public bool FinishedSpawning()
    {
        return !isSpawning;
    }


    #endregion
}

[System.Serializable]
public class PlatformData
{
    public GameObject gameObject;
    public Vector3 position;
    public float spawnTime;
}