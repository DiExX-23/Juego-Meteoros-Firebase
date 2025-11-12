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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (GameServices.Instance == null)
        {
            var servicesGo = new GameObject("GameServices");
            servicesGo.AddComponent<GameServices>();
        }
    }

    public void HandlePlayerHit()
    {
        if (isTransitioning || isGameOver) return;

        isTransitioning = true;
        isGameOver = true;

        activeSpawner?.StopAllCoroutines();
        if (activeSpawner != null) activeSpawner.isSpawning = false;

        string playerName = PlayerProfile.Instance?.playerName ?? "Player";
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.currentScore : 0;

        GameSession.Instance?.EndSession(score);

        float sessionDuration = GameSession.Instance?.sessionDuration ?? 0f;
        int dodged = GameSession.Instance?.meteorsDodged ?? 0;
        int attempts = GameSession.Instance?.attempts ?? 1;

        if (FirestoreService.Instance != null)
            FireAndForgetSave(playerName, score, sessionDuration, dodged, attempts);
        else
            LeaderboardManager.Instance?.AddEntry(playerName, score);

        _ = ProceedToLossSceneAsync();
    }

    private async void FireAndForgetSave(string playerName, int score, float duration, int dodged, int attempts)
    {
        try
        {
            await FirestoreService.Instance.AddHighscoreAsync(playerName, score, duration, dodged, attempts);
        }
        catch
        {
            LeaderboardManager.Instance?.AddEntry(playerName, score);
        }
    }

    private async Task ProceedToLossSceneAsync()
    {
        await Task.Delay(50);
        if (!string.IsNullOrEmpty(lossSceneName))
        {
            var loadOp = SceneManager.LoadSceneAsync(lossSceneName, LoadSceneMode.Single);
            while (!loadOp.isDone) await Task.Yield();
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}