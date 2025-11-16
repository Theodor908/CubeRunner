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

    private Transform playerTransform;
    private CubePlayerController playerController;
    private PlatformSpawner spawnerController;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("No GameObject with tag 'Player' found.");
        }

        spawner = GameObject.FindGameObjectWithTag("PlatformSpawner");

        if (spawner == null)
        {
            Debug.LogError("No GameObject with tag 'PlatformSpawner' found.");
        }

        playerTransform = player.transform;
        playerController = player.GetComponent<CubePlayerController>();

        spawnerController = spawner.GetComponent<PlatformSpawner>();
    }

    private void Update()
    {
        CheckPlayerFall();
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
        playerController.SetFreeze(true);
        endgameUI.ShowEndgameUI();

    }

    public void ResetGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
