using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject ControlsPanel;
    public bool showingControls = false;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            SceneManager.LoadScene("Game");
        }
        else if (Input.GetKeyDown(KeyCode.C)) {
            ShowControls();
        }
    }

    public void PlayOnClick()
    {
        SceneManager.LoadScene("Game");
    }

    public void ShowControls()
    {
        showingControls = !showingControls;
        ControlsPanel.SetActive(showingControls);
    }

    public void ExitOnClick()
    {
        Application.Quit();
    }
}
