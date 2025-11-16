using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Game Settings")]
    [SerializeField] private int deathZoneYOffset;
    [SerializeField] private float deathZoneTimeUpdateCooldown;

    private float timeSinceLastUpdateTime = 0;
    private int yDeathZone;
    private bool deathZoneSet = false;

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject spawner;
    [SerializeField] private EndgameUI endgameUI;

    private Transform playerTransform;
    private CubePlayerController playerController;
    private PlatformSpawner spawnerController;

    private float lastPlatformY;

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
        if (deathZoneSet == false)
        {
            GetStartDeathZone();
        }

        UpdateDeathZonePostion();
        CheckPlayerFall();
    }

    private void GetStartDeathZone()
    {
        while (spawnerController.FinishedSpawning() == false)
        {
        }

        yDeathZone = Mathf.FloorToInt(lastPlatformY) - deathZoneYOffset;
        timeSinceLastUpdateTime = Time.deltaTime;
        deathZoneSet = true;
    }

    private void UpdateDeathZonePostion()
    {

        if (Time.deltaTime - timeSinceLastUpdateTime > deathZoneTimeUpdateCooldown)
        {
            while (spawnerController.FinishedSpawning() == false) 
            { 
            }
            yDeathZone = Mathf.FloorToInt(lastPlatformY) - deathZoneYOffset;
            timeSinceLastUpdateTime = Time.deltaTime;
            Debug.Log("Death Zone Updated to Y = " + yDeathZone);
        }

    }

    private void CheckPlayerFall()
    {
        if (playerTransform.position.y < yDeathZone && spawnerController.FinishedSpawning())
        {
            PlayerLost();
        }
    }

    public void SetLastPlatformY(float y)
    {
        lastPlatformY = y;
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
