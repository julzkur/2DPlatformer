using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenUI : MonoBehaviour
{
    public static WinScreenUI Instance;
    AudioManager audioManager;

    public GameObject winScreenPanel;

    void Awake()
    {
        Instance = this;
        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
        }
        winScreenPanel.SetActive(false);
    }

    public void ShowWinScreen()
    {
        audioManager.PlaySFX(audioManager.win);
        Time.timeScale = 0f;
        winScreenPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}