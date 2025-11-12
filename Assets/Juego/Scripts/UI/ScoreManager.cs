using UnityEngine;
using TMPro;
using System.Threading.Tasks;

[DefaultExecutionOrder(-1)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float pointsPerSecond = 5f;

    public int currentScore { get; private set; }
    private bool isRunning;
    private float scoreAccumulator;

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
            await Task.Delay(1000);
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
        string playerName = PlayerProfile.Instance?.playerName ?? "Player";
        scoreText.text = $"{playerName} - Score: {currentScore}";
    }

    public void ResetScore()
    {
        currentScore = 0;
        scoreAccumulator = 0f;
        UpdateScoreDisplay();
        StartRunning();
    }

    public void StartRunning()
    {
        if (isRunning) return;
        isRunning = true;
        GameSession.Instance?.StartSession();
    }

    public void StopRunning()
    {
        if (!isRunning) return;
        isRunning = false;

        GameSession.Instance?.EndSession(currentScore);

        float sessionDuration = GameSession.Instance?.sessionDuration ?? 0f;
        int meteorsDodged = GameSession.Instance?.meteorsDodged ?? 0;
        int attempts = GameSession.Instance?.attempts ?? 1;

        if (FirestoreService.Instance != null)
            SaveScoreToFirebase(currentScore, sessionDuration, meteorsDodged, attempts);
    }

    public void RegisterMeteorDodged(int dodgePoints)
    {
        AddPoints(dodgePoints);
        GameSession.Instance?.RegisterMeteorDodged();
    }

    private async void SaveScoreToFirebase(int score, float sessionDuration, int meteorsDodged, int attempts)
    {
        try
        {
            string playerName = PlayerProfile.Instance?.playerName ?? "Player";
            await FirestoreService.Instance.AddHighscoreAsync(playerName, score, sessionDuration, meteorsDodged, attempts);
        }
        catch
        {
            LeaderboardManager.Instance?.AddEntry(PlayerProfile.Instance?.playerName ?? "Player", score);
        }
    }
}