using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    private DateTime startTime;
    public int meteorsDodged { get; private set; }
    public int attempts { get; private set; }
    public float sessionDuration { get; private set; }
    public int scoreRecorded { get; private set; }

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

    public void StartSession()
    {
        startTime = DateTime.UtcNow;
        meteorsDodged = 0;
        sessionDuration = 0f;
        scoreRecorded = 0;
        attempts = Mathf.Max(1, attempts);
    }

    public void RegisterMeteorDodged()
    {
        meteorsDodged++;
    }

    public void IncrementAttempts()
    {
        attempts++;
    }

    public void EndSession(int score)
    {
        sessionDuration = (float)(DateTime.UtcNow - startTime).TotalSeconds;
        scoreRecorded = score;
    }
}