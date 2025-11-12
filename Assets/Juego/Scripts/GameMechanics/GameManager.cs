using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public MeteorSpawner activeSpawner;
    public string lossSceneName = "Clasificacion";

    private bool isTransitioning = false;
    private bool isGameOver = false;

    private void Start()
    {
        if (GameServices.Instance == null)
        {
            var servicesGo = new GameObject("GameServices");
            servicesGo.AddComponent<GameServices>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HandlePlayerHit()
    {
        if (isTransitioning || isGameOver) return;

        isTransitioning = true;
        isGameOver = true;

        if (activeSpawner != null)
        {
            activeSpawner.isSpawning = false;
            activeSpawner.StopAllCoroutines();
        }

        string playerName = PlayerProfile.Instance != null ? PlayerProfile.Instance.playerName : "Player";
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.currentScore : 0;

        if (FirestoreService.Instance != null)
        {
            try
            {
                float duration = GameSession.Instance != null ? GameSession.Instance.GetSessionDuration() : 0f;
                int dodged = GameSession.Instance != null ? GameSession.Instance.meteorsDodged : 0;
                int attempts = GameSession.Instance != null ? GameSession.Instance.attempts : 1;

                FireAndForgetSave(playerName, score, duration, dodged, attempts);
            }
            catch (Exception)
            {
                if (LeaderboardManager.Instance == null)
                {
                    var go = new GameObject("LeaderboardManager");
                    go.AddComponent<LeaderboardManager>();
                }
                LeaderboardManager.Instance?.AddEntry(playerName, score);
            }
        }

        _ = ProceedToLossSceneAsync();
    }

    private async void FireAndForgetSave(string playerName, int score, float duration, int dodged, int attempts)
    {
        try
        {
            var task = FirestoreService.Instance.AddHighscoreAsync(playerName, score, duration, dodged, attempts);
            await task;
        }
        catch (Exception)
        {
            if (LeaderboardManager.Instance == null)
            {
                var go = new GameObject("LeaderboardManager");
                go.AddComponent<LeaderboardManager>();
            }
            LeaderboardManager.Instance?.AddEntry(playerName, score);
        }
    }

    private async Task ProceedToLossSceneAsync()
    {
        await Task.Delay(50);
        Debug.Log($"Loading loss scene: {lossSceneName}");
        if (!string.IsNullOrEmpty(lossSceneName))
        {
            var loadOp = SceneManager.LoadSceneAsync(lossSceneName, LoadSceneMode.Single);
            if (loadOp != null)
            {
                while (!loadOp.isDone)
                    await Task.Yield();
                return;
            }
        }
        ReloadCurrentScene();
    }

    async System.Threading.Tasks.Task SaveAndProceed(FirestoreService svc, string playerName, int score, float duration, int dodged, int attempts)
    {
        try
        {
            await svc.AddHighscoreAsync(playerName, score, duration, dodged, attempts);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Firestore save failed: " + ex);
        }
    }

    void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}