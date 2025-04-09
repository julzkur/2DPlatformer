using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused)
        {
           if (Input.GetKeyDown(KeyCode.R))
            {
                ResumeGame();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ExitToMenu();
            } 
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        pauseUI.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
