using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.P)) {
            SceneManager.LoadScene("Game");
        }
    }
}
