using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    DateTime startTime;
    public int meteorsDodged { get; private set; }
    public int attempts { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartSession();
    }

    public void StartSession()
    {
        startTime = DateTime.UtcNow;
        meteorsDodged = 0;
        attempts = Mathf.Max(1, attempts);
    }

    public void RegisterMeteorDodged()
    {
        meteorsDodged++;
    }

    public float GetSessionDuration()
    {
        return (float)(DateTime.UtcNow - startTime).TotalSeconds;
    }

    public void IncrementAttempts()
    {
        attempts++;
    }
}
