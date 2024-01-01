using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Door : MonoBehaviour
{
    public Text doorPrompt;
    public Text completeText;
    public Image fadePanel;
    private string completedSceneName = "GameCompleted";

    private bool isPlayerNear = false;

    private void Start()
    {
        doorPrompt.text = "";
        fadePanel.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            doorPrompt.text = "Press [E] to open the door";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            doorPrompt.text = "";
        }
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Access keysCollected and totalKeys via the KeyManager instance
            if (KeyManager.Instance.keysCollected >= KeyManager.Instance.totalKeys)
            {
                StartCoroutine(OpenDoor());
            }
            else
            {
                doorPrompt.text = "Not enough keys";
                StartCoroutine(HidePromptAfterDelay());
            }
        }
    }


    private IEnumerator OpenDoor()
    {
        completeText.text = "This light, it is blinding me. I am free at last.";
        fadePanel.gameObject.SetActive(true);
        float fadeDuration = 2f;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            fadePanel.color = new Color(1f, 1f, 1f, t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(completedSceneName);
    }

    private IEnumerator HidePromptAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        doorPrompt.text = "";
    }
}
