using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class RemoteHighscore
{
    [FirestoreProperty] public string id { get; set; }
    [FirestoreProperty] public string name { get; set; }
    [FirestoreProperty] public int score { get; set; }
    [FirestoreProperty] public long timestampMillis { get; set; }
    [FirestoreProperty] public float sessionDuration { get; set; }
    [FirestoreProperty] public int meteorsDodged { get; set; }
    [FirestoreProperty] public int attempts { get; set; }
}

public class FirestoreService : MonoBehaviour
{
    public static FirestoreService Instance { get; private set; }

    private FirebaseFirestore db;
    private ListenerRegistration listener;
    private bool initialized;

    public event Action<List<RemoteHighscore>> OnHighscoresUpdated;

    private async void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
        await InitializeFirestoreWithRetry();
    }

    private async Task InitializeFirestoreWithRetry()
    {
        int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Debug.Log($"Intento {attempt}/{maxAttempts} de inicializar Firestore...");

                if (!FirebaseInitializer.IsInitialized)
                {
#if UNITY_2023_1_OR_NEWER
                    if (UnityEngine.Object.FindFirstObjectByType<FirebaseInitializer>() == null)
#else
                    if (UnityEngine.Object.FindObjectOfType<FirebaseInitializer>() == null)
#endif
                    {
                        var go = new GameObject("FirebaseInitializer");
                        go.AddComponent<FirebaseInitializer>();
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        if (FirebaseInitializer.IsInitialized) break;
                        await Task.Delay(100);
                    }
                }

                if (!FirebaseInitializer.IsInitialized)
                    throw new Exception("Firebase aún no está inicializado");

                db = FirebaseFirestore.DefaultInstance;
                if (db != null)
                {
                    initialized = true;
                    Debug.Log($"Firestore inicializado correctamente en el intento {attempt}");
                    return;
                }

                throw new Exception("No se pudo obtener la instancia de Firestore");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error de inicialización de Firestore (intento {attempt}/{maxAttempts}): {ex.Message}");
                if (attempt == maxAttempts)
                    Debug.LogError("Falló la inicialización de Firestore después de todos los intentos");
                else
                    await Task.Delay(1000);
            }
        }
    }

    private async Task EnsureInitialized()
    {
        if (initialized && db != null) return;
        await InitializeFirestoreWithRetry();
    }

    public async Task AddHighscoreAsync(string playerName, int score, float sessionDuration, int meteorsDodged, int attempts)
    {
        if (db == null) throw new Exception("Firestore not initialized");

        var data = new Dictionary<string, object>
        {
            { "name", string.IsNullOrEmpty(playerName) ? "Player" : playerName },
            { "score", score },
            { "sessionDuration", sessionDuration },
            { "meteorsDodged", meteorsDodged },
            { "attempts", attempts },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        await db.Collection("Highscores").AddAsync(data);
    }

    public async Task<List<RemoteHighscore>> GetTopHighscoresAsync(int topN = 50)
    {
        await EnsureInitialized();
        var list = new List<RemoteHighscore>();
        var q = db.Collection("Highscores").OrderByDescending("score").Limit(topN);
        var snapshot = await q.GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            list.Add(ParseDoc(doc));
        }
        return list;
    }

    private RemoteHighscore ParseDoc(DocumentSnapshot doc)
    {
        var r = new RemoteHighscore();
        r.id = doc.Id;
        var dict = doc.ToDictionary();
        if (dict.TryGetValue("name", out var tmp)) r.name = tmp as string;
        if (dict.TryGetValue("score", out tmp)) r.score = Convert.ToInt32(tmp);
        if (dict.TryGetValue("sessionDuration", out tmp)) r.sessionDuration = Convert.ToSingle(tmp);
        if (dict.TryGetValue("meteorsDodged", out tmp)) r.meteorsDodged = Convert.ToInt32(tmp);
        if (dict.TryGetValue("attempts", out tmp)) r.attempts = Convert.ToInt32(tmp);
        if (dict.TryGetValue("timestamp", out tmp) && tmp is Timestamp ts)
            r.timestampMillis = ts.ToDateTime().ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond;
        return r;
    }

    public async void StartListeningTop(int topN = 50)
    {
        await EnsureInitialized();
        try
        {
            StopListening();

            if (db == null)
            {
                Debug.LogError("Firestore no inicializado, no se puede iniciar escucha.");
                return;
            }

            var query = db.Collection("Highscores").OrderByDescending("score").Limit(topN);

            listener = query.Listen(snapshot =>
            {
                try
                {
                    if (snapshot == null || snapshot.Count == 0)
                    {
                        OnHighscoresUpdated?.Invoke(new List<RemoteHighscore>());
                        return;
                    }

                    var list = new List<RemoteHighscore>();
                    foreach (var doc in snapshot.Documents)
                        list.Add(ParseDoc(doc));

                    OnHighscoresUpdated?.Invoke(list);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error procesando datos de Firestore: {ex.Message}");
                }
            });

            Debug.Log("🔥 Escucha activa de puntuaciones (Leaderboard actualizándose en tiempo real)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al iniciar la escucha de puntuaciones: {ex.Message}");
        }
    }

    public void StopListening()
    {
        try
        {
            listener?.Stop();
            listener = null;
        }
        catch
        {
            listener = null;
        }
    }
}