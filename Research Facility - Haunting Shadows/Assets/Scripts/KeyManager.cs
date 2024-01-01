using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;

    public Text keyPromptText;
    public Text keysCollectedText;
    public int totalKeys = 5;
    public int keysCollected = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        keyPromptText.gameObject.SetActive(false);
        keysCollectedText.gameObject.SetActive(false);
        UpdateKeysCollectedText();
    }

    public void HideKeyPrompt()
    {
        keyPromptText.gameObject.SetActive(false);
    }
    public void ShowKeyPrompt()
    {
        keyPromptText.gameObject.SetActive(true);
        StartCoroutine(HideTextAfterDelay(keyPromptText));
    }

    public void AddKey()
    {
        keysCollected++;
        UpdateKeysCollectedText();
        keysCollectedText.gameObject.SetActive(true);
        StartCoroutine(HideTextAfterDelay(keysCollectedText));

        if (keysCollected >= totalKeys)
        {
            // Logic for all keys collected
            Debug.Log("All keys collected! You can now open the door.");
        }
    }

    private IEnumerator HideTextAfterDelay(Text textElement)
    {
        yield return new WaitForSeconds(2f);
        textElement.gameObject.SetActive(false);
    }

    private void UpdateKeysCollectedText()
    {
        keysCollectedText.text = $"{keysCollected} / {totalKeys} keys collected";
    }
}
