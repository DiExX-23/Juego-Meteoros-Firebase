using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class RemoteHighscore
{
    [FirestoreProperty]
    public string id { get; set; }
    [FirestoreProperty]
    public string name { get; set; }
    [FirestoreProperty]
    public int score { get; set; }
    [FirestoreProperty]
    public long timestampMillis { get; set; }
    [FirestoreProperty]
    public float sessionDuration { get; set; }
    [FirestoreProperty]
    public int meteorsDodged { get; set; }
    [FirestoreProperty]
    public int attempts { get; set; }
}

public class FirestoreService : MonoBehaviour
{
    public static FirestoreService Instance { get; private set; }
    FirebaseFirestore db;
    ListenerRegistration listener;

    public event Action<List<RemoteHighscore>> OnHighscoresUpdated;

    async void Start()
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
        int maxAttempts = 5; // Aumentamos los intentos
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Debug.Log($"Intento {attempt}/{maxAttempts} de inicializar Firestore...");

                // Verificar si Firebase está inicializado
                if (!FirebaseInitializer.IsInitialized)
                {
                    // Si no hay FirebaseInitializer, lo creamos
                    if (FindObjectOfType<FirebaseInitializer>() == null)
                    {
                        Debug.Log("Creando FirebaseInitializer...");
                        var go = new GameObject("FirebaseInitializer");
                        go.AddComponent<FirebaseInitializer>();
                    }

                    // Esperar a que se inicialice
                    for (int i = 0; i < 20; i++)
                    {
                        if (FirebaseInitializer.IsInitialized) break;
                        await Task.Delay(100);
                    }
                }

                if (!FirebaseInitializer.IsInitialized)
                {
                    throw new Exception("Firebase aún no está inicializado");
                }

                // Intentar obtener la instancia de Firestore
                db = FirebaseFirestore.DefaultInstance;
                if (db != null)
                {
                    Debug.Log($"Firestore inicializado correctamente en el intento {attempt}");
                    return;
                }
                
                throw new Exception("No se pudo obtener la instancia de Firestore");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error de inicialización de Firestore (intento {attempt}/{maxAttempts}): {ex.Message}");
                if (attempt == maxAttempts)
                {
                    Debug.LogError("Falló la inicialización de Firestore después de todos los intentos");
                }
                else
                {
                    await Task.Delay(1000); // Esperar antes del siguiente intento
                }
            }
        }
    }

    private async Task InitializeFirestore()
    {
        try
        {
            var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                Debug.Log("Firestore initialized successfully");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Firestore: {ex}");
        }
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
        var list = new List<RemoteHighscore>();
        var q = db.Collection("Highscores").OrderByDescending("score").Limit(topN);
        var snapshot = await q.GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            list.Add(ParseDoc(doc));
        }
        return list;
    }

    RemoteHighscore ParseDoc(DocumentSnapshot doc)
    {
        var r = new RemoteHighscore();
        r.id = doc.Id;
        var dict = doc.ToDictionary();
        object tmp;
        if (dict.TryGetValue("name", out tmp)) r.name = tmp as string;
        if (dict.TryGetValue("score", out tmp)) r.score = Convert.ToInt32(tmp);
        if (dict.TryGetValue("sessionDuration", out tmp)) r.sessionDuration = Convert.ToSingle(tmp);
        if (dict.TryGetValue("meteorsDodged", out tmp)) r.meteorsDodged = Convert.ToInt32(tmp);
        if (dict.TryGetValue("attempts", out tmp)) r.attempts = Convert.ToInt32(tmp);
        if (dict.TryGetValue("timestamp", out tmp) && tmp is Timestamp ts) r.timestampMillis = ts.ToDateTime().ToUniversalTime().Ticks / TimeSpan.TicksPerMillisecond;
        return r;
    }

    public async void StartListeningTop(int topN = 50)
    {
        try
        {
            StopListening();
            
            // Si db es nulo, intentar inicializar
            if (db == null)
            {
                Debug.Log("Firestore no inicializado, intentando inicializar...");
                await InitializeFirestore();
                
                // Verificar nuevamente después de intentar inicializar
                if (db == null)
                {
                    Debug.LogError("No se pudo inicializar Firestore");
                    return;
                }
            }

            var q = db.Collection("Highscores").OrderByDescending("score").Limit(topN);
            listener = q.Listen(snapshot =>
            {
                try
                {
                    var list = new List<RemoteHighscore>();
                    foreach (var doc in snapshot.Documents)
                    {
                        list.Add(ParseDoc(doc));
                    }
                    OnHighscoresUpdated?.Invoke(list);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error procesando datos de Firestore: {ex.Message}");
                }
            });
            
            Debug.Log("Escucha de puntuaciones iniciada correctamente");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al iniciar la escucha de puntuaciones: {ex.Message}");
        }
    }

    public void StopListening()
    {
        try { listener?.Stop(); listener = null; } catch { listener = null; }
    }
}
