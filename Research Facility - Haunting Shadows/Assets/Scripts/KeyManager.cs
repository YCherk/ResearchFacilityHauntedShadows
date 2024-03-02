
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KeyManager : MonoBehaviour
{
    // Singleton instance to access the KeyManager globally without duplicates
    public static KeyManager Instance;

    // UI text that prompts the player to pick up a key
    public Text keyPromptText;
    // UI text displaying the number of keys collected
    public Text keysCollectedText;
    // Total number of keys that can be collected in the game
    public int totalKeys = 5;
    // Counter for the keys that have been collected so far
    public int keysCollected = 0;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Ensures that only one instance of KeyManager exists in the scene
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Destroys this object if an instance already exists to avoid duplicates
            Destroy(gameObject);
        }

        // Initially hides the key prompt and collected keys text on game start
        keyPromptText.gameObject.SetActive(false);
        keysCollectedText.gameObject.SetActive(false);
        // Updates the UI to reflect the initial state of keys collected
        UpdateKeysCollectedText();
    }

    // Hides the key prompt UI element
    public void HideKeyPrompt()
    {
        keyPromptText.gameObject.SetActive(false);
    }
    // Shows the key prompt UI element and hides it after a delay
    public void ShowKeyPrompt()
    {
        keyPromptText.gameObject.SetActive(true);
        // Starts a coroutine to hide the text after a delay
        StartCoroutine(HideTextAfterDelay(keyPromptText));
    }

    // Increments the keysCollected counter and updates the UI accordingly
    public void AddKey()
    {
        keysCollected++;
        UpdateKeysCollectedText();
        keysCollectedText.gameObject.SetActive(true);
        // Starts a coroutine to hide the collected keys text after a delay
        StartCoroutine(HideTextAfterDelay(keysCollectedText));

    }

    // Coroutine to hide a text UI element after a specified delay
    private IEnumerator HideTextAfterDelay(Text textElement)
    {
        // Waits for 2 seconds
        yield return new WaitForSeconds(2f);
        // Then hides the text element
        textElement.gameObject.SetActive(false);
    }

    // Updates the keys collected text to show current progress
    private void UpdateKeysCollectedText()
    {
        // Formats the text to show how many keys have been collected out of the total
        keysCollectedText.text = $"{keysCollected} / {totalKeys} keys collected";
    }
}
