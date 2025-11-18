using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Settings")]
    [SerializeField] private int deathZoneYOffset;
    [SerializeField] private float deathZoneTimeUpdateCooldown;

    private int yDeathZone = int.MinValue;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject spawner;
    [SerializeField] private EndgameUI endgameUI;
    [SerializeField] private PlayerSpawner playerSpawner;

    private Transform playerTransform;
    private CubePlayerController playerController;
    private PlatformSpawner spawnerController;

    private bool playerSpawnerInitialized = false;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        spawner = GameObject.FindGameObjectWithTag("PlatformSpawner");

        if (spawner == null)
        {
            Debug.LogError("No GameObject with tag 'PlatformSpawner' found.");
        }

        spawnerController = spawner.GetComponent<PlatformSpawner>();
    }

    private void Update()
    {

        if(playerSpawner == null)
        {
             playerSpawner = FindAnyObjectByType<PlayerSpawner>();
        }

        if (playerTransform != null)
            CheckPlayerFall();

        if(player == null)
        {
            if (playerSpawner.FoundSpawnPoint()  && playerSpawnerInitialized == false)
            {
                playerSpawner.SpawnPlayer(Quaternion.identity, false);
                playerSpawnerInitialized= true;
            }
            player = GameObject.FindGameObjectWithTag("Player");
            if(player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<CubePlayerController>();
            }
        }
    }

    public void UpdateDeathZonePostion(float lastPlatformY)
    {
        yDeathZone = Mathf.FloorToInt(lastPlatformY) - deathZoneYOffset;
    }

    private void CheckPlayerFall()
    {
        if (playerTransform.position.y < yDeathZone && spawnerController.FinishedSpawning())
        {
            PlayerLost();
        }
    }

    public void PlayerLost()
    {
        Debug.Log("Player lost wow");
        playerController.SetFreeze(true);
        if (endgameUI == null)
        { 
        }
        Debug.Log("Showing endgame");
        endgameUI.ShowEndgameUI();

    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
