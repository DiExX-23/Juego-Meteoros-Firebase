using UnityEngine;
using System.Threading.Tasks;

[DefaultExecutionOrder(-100)]
public class GameServices : MonoBehaviour
{
    public static GameServices Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeServices();
    }
    
    private async void InitializeServices()
    {
        // Crear FirestoreService si no existe
        if (FirestoreService.Instance == null)
        {
            GameObject firestoreGo = new GameObject("FirestoreService");
            firestoreGo.transform.SetParent(transform);
            await Task.Yield(); // Esperar un frame para asegurar que Awake se ejecute
            FirestoreService firestore = firestoreGo.AddComponent<FirestoreService>();
        }

        // Crear LeaderboardManager si no existe
        if (LeaderboardManager.Instance == null)
        {
            GameObject leaderboardGo = new GameObject("LeaderboardManager");
            leaderboardGo.transform.SetParent(transform);
            leaderboardGo.AddComponent<LeaderboardManager>();
        }

        // Crear PlayerProfile si no existe
        if (PlayerProfile.Instance == null)
        {
            GameObject profileGo = new GameObject("PlayerProfile");
            profileGo.transform.SetParent(transform);
            profileGo.AddComponent<PlayerProfile>();
        }
    }
}