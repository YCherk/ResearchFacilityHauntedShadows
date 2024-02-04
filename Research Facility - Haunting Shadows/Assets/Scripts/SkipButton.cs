using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkipButton : MonoBehaviour
{
    public GameObject loadingCanvas; // Assign in Inspector
    public Text loadingText; // Assign the "Loading" Text component in Inspector
    private string baseLoadingText = "Loading";
    private int dotCount = 0;

    public void Skip()
    {
        loadingCanvas.SetActive(true); // Show the loading canvas
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(LoadSceneAsync("Start")); // Start loading the scene asynchronously
    }

    IEnumerator AnimateLoadingText()
    {
        while (true) // Loop to keep animating until the scene is loaded
        {
            dotCount = (dotCount + 1) % 4; // Cycle dotCount from 0 to 3
            loadingText.text = baseLoadingText + new string('.', dotCount); // Update text
            yield return new WaitForSeconds(0.5f); // Wait half a second before next update
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // Optional: Update any loading progress UI here
            yield return null;
        }

        // Optionally, stop the loading text animation once the scene is ready
         StopCoroutine("AnimateLoadingText");
         loadingText.text = baseLoadingText; // Reset text if needed
    }

    void Start()
    {
        loadingCanvas.SetActive(false); // Ensure the loading canvas is hidden at start
    }
}
