using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // Singleton instance to allow easy access from other scripts
    public static UIManager Instance { get; private set; }

    // UI Text elements for different types of in-game messages and prompts
    public Text collectText; // Displays general messages to the player
    public Text transcriptText; // Shows transcriptions of spoken words (e.g., voice commands or spirit responses)
    public Text speakPrompt; // Prompt for encouraging the player to speak to the spirit
    public Text keyPromptText; // Prompt for collecting keys
    public Text doorPromptText; // Prompt for interacting with doors

    // Reference to the script handling player voice interactions
    public PlayerVoiceInteraction playerVoiceInteraction;

    // Coroutine variable for managing the transcript display's visibility
    private Coroutine hideTranscriptCoroutine;

    private void Awake()
    {
        // Ensures that there is only one instance of the UIManager in the scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes UIManager persistent across different scenes
        }
        else
        {
            Destroy(gameObject); // Destroys duplicate instances
        }
    }

    private void Start()
    {
        // Set up event listeners for player voice interaction events, if the component is available
        if (playerVoiceInteraction != null)
        {
            // Subscribing to events from the PlayerVoiceInteraction script
            playerVoiceInteraction.OnPlayerSpeech += HandleTranscriptDisplay;
            playerVoiceInteraction.ShowSpeakPrompt += ShowSpeakPrompt;
            playerVoiceInteraction.HideSpeakPrompt += HideSpeakPrompt;
        }
        else
        {
            Debug.LogWarning("UIManager: PlayerVoiceInteraction reference not set.");
        }
    }

    // Displays a temporary message on the UI
    public void ShowMessage(string message, float duration)
    {
        if (collectText != null)
        {
            StartCoroutine(DisplayMessage(message, duration));
        }
        else
        {
            Debug.LogWarning("UIManager: CollectText UI element not set.");
        }
    }

    // Coroutine for displaying a message for a specified duration
    private IEnumerator DisplayMessage(string message, float duration)
    {
        collectText.text = message;
        collectText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        collectText.gameObject.SetActive(false);
    }

    // Displays a prompt for key collection
    public void ShowKeyPrompt(string message)
    {
        if (keyPromptText != null)
        {
            keyPromptText.text = message;
            keyPromptText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UIManager: KeyPromptText UI element not set.");
        }
    }

    // Hides the key collection prompt
    public void HideKeyPrompt()
    {
        if (keyPromptText != null)
        {
            keyPromptText.gameObject.SetActive(false);
        }
    }

    // Displays the transcription of player speech and schedules its hiding
    private void HandleTranscriptDisplay(string text)
    {
        if (transcriptText != null)
        {
            if (hideTranscriptCoroutine != null)
            {
                StopCoroutine(hideTranscriptCoroutine); // Stops the previous hiding coroutine to reset the timer
            }

            transcriptText.text = text;
            // Start a new coroutine to hide the transcript after a delay
            hideTranscriptCoroutine = StartCoroutine(HideTranscriptAfterDelay(6f));
        }
        else
        {
            Debug.LogWarning("UIManager: TranscriptText UI element not set.");
        }
    }

    // Coroutine to hide the transcript text after a specified delay
    IEnumerator HideTranscriptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transcriptText.text = ""; // Clears the transcript text
    }

    // Displays the "Speak to Spirit" prompt
    public void ShowSpeakPrompt()
    {
        if (speakPrompt != null)
        {
            speakPrompt.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("SpeakPrompt is not assigned in UIManager.");
        }
    }

    // Hides the "Speak to Spirit" prompt
    public void HideSpeakPrompt()
    {
        if (speakPrompt != null)
        {
            speakPrompt.gameObject.SetActive(false);
        }
    }

    // Shows a prompt when near a door that can be interacted with
    public void ShowDoorPrompt(string message)
    {
        if (doorPromptText != null)
        {
            doorPromptText.text = message;
            doorPromptText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UIManager: DoorPromptText UI element not set.");
        }
    }

    // Hides the door interaction prompt
    public void HideDoorPrompt()
    {
        if (doorPromptText != null)
        {
            doorPromptText.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the player voice interaction events to prevent memory leaks
        if (playerVoiceInteraction != null)
        {
            playerVoiceInteraction.OnPlayerSpeech -= HandleTranscriptDisplay;
            playerVoiceInteraction.ShowSpeakPrompt -= ShowSpeakPrompt;
            playerVoiceInteraction.HideSpeakPrompt -= HideSpeakPrompt;
        }
    }
}
