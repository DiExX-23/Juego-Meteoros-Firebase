using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public int score;
    public string date;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    public int maxEntries = 10;
    const string PREFS_KEY = "leaderboard_v1";

    List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.parent = null;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddEntry(string playerName, int score)
    {
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";
        var e = new LeaderboardEntry { name = playerName, score = score, date = System.DateTime.UtcNow.ToString("s") };
        entries.Add(e);
        entries = entries.OrderByDescending(x => x.score).ThenBy(x => x.date).Take(maxEntries).ToList();
        Save();
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return new List<LeaderboardEntry>(entries);
    }

    public void Clear()
    {
        entries.Clear();
        Save();
    }

    void Save()
    {
        var wrapper = new Wrapper { entries = entries };
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        var json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (string.IsNullOrEmpty(json))
        {
            entries = new List<LeaderboardEntry>();
            return;
        }
        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        entries = wrapper.entries ?? new List<LeaderboardEntry>();
    }

    [System.Serializable]
    class Wrapper
    {
        public List<LeaderboardEntry> entries;
    }
}
