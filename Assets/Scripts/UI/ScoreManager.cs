// ScoreManager.cs
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public TextMeshProUGUI scoreText;
    public int currentScore { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        UpdateScoreDisplay();
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore.ToString();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }
}
