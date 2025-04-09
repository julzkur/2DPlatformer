using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button Play;
    public Button Exit;

    private void Awake()
    {
        if (Play)
        {
            Play.onClick.AddListener(PlayOnClick);
        }
        
        if (Exit)
        {
            Exit.onClick.AddListener(ExitOnClick);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            SceneManager.LoadScene("Game");
        }
    }

    public void PlayOnClick()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitOnClick()
    {
        Application.Quit();
    }
}
