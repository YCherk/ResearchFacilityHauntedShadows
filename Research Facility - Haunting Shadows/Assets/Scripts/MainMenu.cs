using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the 'Start' scene
        SceneManager.LoadScene("Start");
    }

    public void OpenOptions()
    {
        // Load the 'Options' scene
        SceneManager.LoadScene("Options");
    }

    public void Back()
    {
        // Load the 'Main Menu' scene
        SceneManager.LoadScene("Main Menu");
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();

        // If running in the Unity editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
