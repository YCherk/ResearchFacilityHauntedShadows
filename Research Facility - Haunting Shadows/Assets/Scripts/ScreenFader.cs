using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public float fadeDuration = 2f;
    private Image fadePanel;
    public bool isFading = false;

    void Start()
    {
        fadePanel = GetComponent<Image>();
    }

    public void FadeToGameover()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        isFading = true;
        float timer = 0;

        while (timer <= fadeDuration)
        {
            fadePanel.color = new Color(0, 0, 0, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene("Game Over"); 
    }
}
