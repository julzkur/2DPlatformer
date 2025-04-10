using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenUI : MonoBehaviour
{
    public static WinScreenUI Instance;

    public GameObject winScreenPanel;

    void Awake()
    {
        Instance = this;
        winScreenPanel.SetActive(false);
    }

    public void ShowWinScreen()
    {
        Time.timeScale = 0f;
        winScreenPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}