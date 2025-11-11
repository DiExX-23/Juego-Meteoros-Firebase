using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LeaderboardSimpleUI : MonoBehaviour
{
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

    async void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
        }

        // Esperar a que Firebase esté inicializado
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            // Si no existe FirestoreService, crearlo
            if (FirestoreService.Instance == null)
            {
                var go = new GameObject("FirestoreService");
                go.AddComponent<FirestoreService>();
                await Task.Delay(100); // Pequeña espera para que se inicialice
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
        ClearUI(); // Limpiar UI mientras se inicializa
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
        
        for (int i = 0; i < 10; i++) // Máximo 10 intentos
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
            if (ranksText != null) ranksText.text = "";
            if (namesText != null) namesText.text = "";
            if (scoresText != null) scoresText.text = "";
            if (attemptsText != null) attemptsText.text = "";
            if (meteorsDodgedText != null) meteorsDodgedText.text = "";
            if (sessionDurationText != null) sessionDurationText.text = "";
            if (timestampText != null) timestampText.text = "";
            return;
        }

        var sbRanks = new System.Text.StringBuilder();
        var sbNames = new System.Text.StringBuilder();
        var sbScores = new System.Text.StringBuilder();
        var sbAttempts = new System.Text.StringBuilder();
        var sbMeteorsDodged = new System.Text.StringBuilder();
        var sbSessionDuration = new System.Text.StringBuilder();
        var sbTimestamp = new System.Text.StringBuilder();

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
            // Asegurarnos de desuscribirnos de Firebase antes de cambiar de escena
            if (FirestoreService.Instance != null)
            {
                FirestoreService.Instance.OnHighscoresUpdated -= OnRemoteHighscores;
                FirestoreService.Instance.StopListening();
            }
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
