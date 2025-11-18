using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject defaultPlayerPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    private GameObject spawnedPlayer;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnPlayer(Quaternion rotation,bool freeze = true)
    {

        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }

        if (playerPrefab == null)
        {
            playerPrefab = defaultPlayerPrefab;
        }
        
        spawnedPlayer = Instantiate(playerPrefab, spawnPosition, rotation);


        CubePlayerController playerController = spawnedPlayer.GetComponent<CubePlayerController>();

        if (playerController != null && freeze)
        {
            playerController.SetFreeze(true);
        }

       
        spawnedPlayer.SetActive(true);

    }

    public void SetPlayerPrefab(GameObject prefab)
    {
        playerPrefab = prefab;
    }

    public bool FoundSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawner");
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.transform.position;
            return true;
        }

        return false;
    }
}
