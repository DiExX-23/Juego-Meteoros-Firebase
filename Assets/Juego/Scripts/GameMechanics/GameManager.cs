using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public MeteorSpawner activeSpawner;
    public string lossSceneName = "Clasificacion";
    
    private bool isTransitioning = false;  // Evitar múltiples transiciones
    private bool isGameOver = false;       // Estado del juego

    private void Start()
    {
        // Asegurar que GameServices existe
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

    public async void HandlePlayerHit()
    {
        if (isTransitioning || isGameOver) return;  // Evitar múltiples llamadas
        
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

                await FirestoreService.Instance.AddHighscoreAsync(playerName, score, duration, dodged, attempts);
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

        await Task.Delay(500);

        Debug.Log($"Loading loss scene: {lossSceneName}");
        if (!string.IsNullOrEmpty(lossSceneName))
        {
            SceneManager.LoadScene(lossSceneName);
            return;
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