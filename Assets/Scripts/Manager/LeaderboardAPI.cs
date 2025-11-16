using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardAPI : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "http://localhost:3000/api";

    private static LeaderboardAPI instance;
    public static LeaderboardAPI Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LeaderboardAPI");
                instance = go.AddComponent<LeaderboardAPI>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator SubmitScore(string playerName, int score, Action onSuccess, Action<string> onError)
    {
        var scoreData = new ScoreSubmission
        {
            player_name = playerName,
            score = score
        };

        string json = JsonUtility.ToJson(scoreData);

        using (UnityWebRequest request = new UnityWebRequest($"{apiBaseUrl}/score/submit", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                string error = $"Failed to submit score: {request.error}";
                onError?.Invoke(error);
            }
        }
    }

    public IEnumerator GetLeaderboard(int limit, Action<LeaderboardEntry[]> onSuccess, Action<string> onError)
    {

        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/leaderboard?limit={limit}"))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<LeaderboardResponse>(request.downloadHandler.text);


                onSuccess?.Invoke(response.leaderboard);
            }
            else
            {
                string error = $"Failed to get leaderboard: {request.error}";
                onError?.Invoke(error);
            }
        }
    }

    public IEnumerator GetPlayerStats(string playerName, Action<PlayerStats> onSuccess, Action<string> onError)
    {

        // Get best score
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/player/{playerName}/score"))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var stats = JsonUtility.FromJson<PlayerStats>(request.downloadHandler.text);


                onSuccess?.Invoke(stats);
            }
            else
            {
                string error = $"Failed to get player stats: {request.error}";
                onError?.Invoke(error);
            }
        }
    }

    public IEnumerator GetPlayerRank(string playerName, Action<PlayerRank> onSuccess, Action<string> onError)
    {

        using (UnityWebRequest request = UnityWebRequest.Get($"{apiBaseUrl}/player/{playerName}/rank"))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var rank = JsonUtility.FromJson<PlayerRank>(request.downloadHandler.text);

                onSuccess?.Invoke(rank);
            }
            else
            {
                string error = $"Failed to get player rank: {request.error}";
                onError?.Invoke(error);
            }
        }
    }

    #region Data Models

    [Serializable]
    private class ScoreSubmission
    {
        public string player_name;
        public int score;
    }

    [Serializable]
    private class LeaderboardResponse
    {
        public LeaderboardEntry[] leaderboard;
        public int count;
    }

    #endregion
}

#region Public Data Models

[Serializable]
public class LeaderboardEntry
{
    public string player_name;
    public int score;
    public int games_played;
    public int rank;
}

[Serializable]
public class PlayerStats
{
    public string player_name;
    public int best_score;
    public int games_played;
    public int average_score;
}

[Serializable]
public class PlayerRank
{
    public string player_name;
    public int best_score;
    public int rank;
}

#endregion