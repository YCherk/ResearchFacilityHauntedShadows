using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class IntroSceneEffect : MonoBehaviour
{
    public Text storyText; // Assign this in the inspector
    public string fullText; // Full story text
    public float typingSpeed = 0.05f; // Speed of typing
    public AudioSource TypeWriterSound;

    private void Start()
    {
        StartCoroutine(TypeOutText());
        TypeWriterSound.Play();
    }
    

    IEnumerator TypeOutText()
    {
        foreach (char letter in fullText.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
            
        }

        
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Start");
    }
}
