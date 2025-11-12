using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LeaderboardSimpleUI : MonoBehaviour
{
    public static LeaderboardSimpleUI Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI ranksText;
    public TextMeshProUGUI namesText;
    public TextMeshProUGUI scoresText;
    public TextMeshProUGUI attemptsText;
    public TextMeshProUGUI meteorsDodgedText;
    public TextMeshProUGUI sessionDurationText;
    public TextMeshProUGUI timestampText;
    public Button backButton;
    public string menuSceneName = "Menu inicio";

    private Canvas persistentCanvas;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Crear un Canvas independiente para UI persistente si no existe
        persistentCanvas = GetComponent<Canvas>();
        if (persistentCanvas == null)
        {
            persistentCanvas = gameObject.AddComponent<Canvas>();
            persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
        }

        transform.SetParent(null); // Desparentar de cualquier Canvas de la escena
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
        }

        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            if (FirestoreService.Instance == null)
            {
                var go = new GameObject("FirestoreService");
                go.AddComponent<FirestoreService>();
                await Task.Delay(100);
            }

            if (FirestoreService.Instance != null)
            {
                FirestoreService.Instance.OnHighscoresUpdated += OnRemoteHighscores;
                FirestoreService.Instance.StartListeningTop(50);
            }
        }

        Refresh();
    }

    async void OnEnable()
    {
        ClearUI();
        await WaitForFirebaseInit();
        Refresh();
    }

    private void ClearUI()
    {
        if (ranksText != null) ranksText.text = "Cargando...";
        if (namesText != null) namesText.text = "";
        if (scoresText != null) scoresText.text = "";
        if (attemptsText != null) attemptsText.text = "";
        if (meteorsDodgedText != null) meteorsDodgedText.text = "";
        if (sessionDurationText != null) sessionDurationText.text = "";
        if (timestampText != null) timestampText.text = "";
    }

    private async Task WaitForFirebaseInit()
    {
        if (FirebaseInitializer.IsInitialized) return;

        for (int i = 0; i < 10; i++)
        {
            if (FirestoreService.Instance != null) return;
            await Task.Delay(100);
        }
    }

    public void Refresh()
    {
        if (FirestoreService.Instance != null && FirebaseInitializer.IsInitialized)
        {
            try
            {
                FirestoreService.Instance.StartListeningTop(50);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error al actualizar clasificación: {ex.Message}");
                ClearUI();
            }
        }
        else
        {
            ClearUI();
        }
    }

    void OnRemoteHighscores(System.Collections.Generic.List<RemoteHighscore> remote)
    {
        if (remote == null || remote.Count == 0)
        {
            ClearUI();
            return;
        }

        var sbRanks = new StringBuilder();
        var sbNames = new StringBuilder();
        var sbScores = new StringBuilder();
        var sbAttempts = new StringBuilder();
        var sbMeteorsDodged = new StringBuilder();
        var sbSessionDuration = new StringBuilder();
        var sbTimestamp = new StringBuilder();

        for (int i = 0; i < remote.Count; i++)
        {
            var e = remote[i];
            sbRanks.Append((i + 1).ToString());
            sbNames.Append(string.IsNullOrEmpty(e.name) ? "Player" : e.name);
            sbScores.Append(e.score.ToString());
            sbAttempts.Append(e.attempts > 0 ? e.attempts.ToString() : "1");
            sbMeteorsDodged.Append(e.meteorsDodged >= 0 ? e.meteorsDodged.ToString() : "0");
            sbSessionDuration.Append(e.sessionDuration >= 0 ? FormatDuration(e.sessionDuration) : "00:00");
            DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(e.timestampMillis);
            sbTimestamp.Append(FormatTimestamp(timestamp));

            if (i < remote.Count - 1)
            {
                sbRanks.AppendLine();
                sbNames.AppendLine();
                sbScores.AppendLine();
                sbAttempts.AppendLine();
                sbMeteorsDodged.AppendLine();
                sbSessionDuration.AppendLine();
                sbTimestamp.AppendLine();
            }
        }

        if (ranksText != null) ranksText.text = sbRanks.ToString();
        if (namesText != null) namesText.text = sbNames.ToString();
        if (scoresText != null) scoresText.text = sbScores.ToString();
        if (attemptsText != null) attemptsText.text = sbAttempts.ToString();
        if (meteorsDodgedText != null) meteorsDodgedText.text = sbMeteorsDodged.ToString();
        if (sessionDurationText != null) sessionDurationText.text = sbSessionDuration.ToString();
        if (timestampText != null) timestampText.text = sbTimestamp.ToString();
    }

    private string FormatDuration(float seconds)
    {
        return seconds.ToString("F1");
    }

    private string FormatTimestamp(DateTime timestamp)
    {
        return timestamp.ToLocalTime().ToString("d 'de' MMMM 'de' yyyy, h:mm:ss tt 'UTC'K");
    }

    void OnBack()
    {
        Debug.Log("OnBack called, returning to: " + menuSceneName);
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            if (FirestoreService.Instance != null)
            {
                FirestoreService.Instance.OnHighscoresUpdated -= OnRemoteHighscores;
                FirestoreService.Instance.StopListening();
            }

            SceneManager.LoadScene(menuSceneName);
        }
    }
}