using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public TMP_InputField nameInput;
    public Button startButton;
    public string gameSceneName = "Juego";

    void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
    }

    void OnStartClicked()
    {
        if (nameInput != null)
        {
            string n = nameInput.text.Trim();
            if (string.IsNullOrEmpty(n)) n = "Player";
            if (PlayerProfile.Instance != null) PlayerProfile.Instance.playerName = n;
            else
            {
                GameObject go = new GameObject("PlayerProfile");
                var pp = go.AddComponent<PlayerProfile>();
                pp.playerName = n;
            }
        }
        if (!string.IsNullOrEmpty(gameSceneName)) SceneManager.LoadScene(gameSceneName);
    }
}
