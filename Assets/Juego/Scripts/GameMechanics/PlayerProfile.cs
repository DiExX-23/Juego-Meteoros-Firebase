using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance { get; private set; }
    public string playerName = "Player";

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
}
