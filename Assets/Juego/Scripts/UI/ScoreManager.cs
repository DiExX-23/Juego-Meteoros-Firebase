using UnityEngine;
using TMPro;
using System.Threading.Tasks;

[DefaultExecutionOrder(-1)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Settings")]
    [SerializeField] private float pointsPerSecond = 1f;
    
    public int currentScore { get; private set; }
    private bool isRunning;
    private float scoreAccumulator;
    private float sessionStartTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        ResetScore();
        await WaitForFirebaseInitialization();
    }

    private async Task WaitForFirebaseInitialization()
    {
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            await Task.Delay(1000); // Dar tiempo para que Firestore se inicialice completamente
        }
    }

    void Update()
    {
        if (!isRunning) return;
        scoreAccumulator += pointsPerSecond * Time.deltaTime;
        if (scoreAccumulator >= 1f)
        {
            int add = Mathf.FloorToInt(scoreAccumulator);
            scoreAccumulator -= add;
            AddPoints(add);
        }
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText == null) return;
        var playerName = PlayerProfile.Instance?.playerName ?? "Player";
        scoreText.text = $"{playerName} - Score: {currentScore}";
    }

    public void ResetScore()
    {
        currentScore = 0;
        scoreAccumulator = 0f;
        sessionStartTime = Time.time;
        UpdateScoreDisplay();
    }

    public void StartRunning()
    {
        isRunning = true;
        scoreAccumulator = 0f;
        sessionStartTime = Time.time;
        if (GameSession.Instance != null)
        {
            GameSession.Instance.StartSession();
        }
    }

    public void StopRunning()
    {
        if (!isRunning) return;
        
        isRunning = false;
        float sessionDuration = Time.time - sessionStartTime;
        
        if (FirestoreService.Instance != null)
        {
            SaveScoreToFirebase(sessionDuration);
        }
    }

    private async void SaveScoreToFirebase(float sessionDuration)
    {
        try
        {
            string playerName = PlayerProfile.Instance != null ? PlayerProfile.Instance.playerName : "Player";
            int meteorsDodged = GameSession.Instance != null ? GameSession.Instance.meteorsDodged : 0;
            int attempts = GameSession.Instance != null ? GameSession.Instance.attempts : 1;

            await FirestoreService.Instance.AddHighscoreAsync(
                playerName,
                currentScore,
                sessionDuration,
                meteorsDodged,
                attempts
            );
        }
        catch (System.Exception)
        {
            if (LeaderboardManager.Instance != null)
            {
                string playerName = PlayerProfile.Instance != null ? PlayerProfile.Instance.playerName : "Player";
                LeaderboardManager.Instance.AddEntry(playerName, currentScore);
            }
        }
    }
}