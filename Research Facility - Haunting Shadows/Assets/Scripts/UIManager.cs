using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text collectText; // Text element for general messages
    public Text transcriptText; // Text element for displaying spoken words
    public Text speakPrompt; // Text element for the "Speak to Spirit" prompt
    public Text keyPromptText; // Text element for the key collection prompt

    public PlayerVoiceInteraction playerVoiceInteraction; // Reference to PlayerVoiceInteraction script

    public Text doorPromptText;

    private Coroutine hideTranscriptCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep UIManager across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // It's assumed that playerVoiceInteraction is assigned in the Unity Editor or elsewhere before Start.
        if (playerVoiceInteraction != null)
        {
            playerVoiceInteraction.OnPlayerSpeech += HandleTranscriptDisplay;
            playerVoiceInteraction.ShowSpeakPrompt += ShowSpeakPrompt;
            playerVoiceInteraction.HideSpeakPrompt += HideSpeakPrompt;
        }
        else
        {
            Debug.LogWarning("UIManager: PlayerVoiceInteraction reference not set.");
        }
    }

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

    private IEnumerator DisplayMessage(string message, float duration)
    {
        collectText.text = message;
        collectText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        collectText.gameObject.SetActive(false);
    }

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

    public void HideKeyPrompt()
    {
        if (keyPromptText != null)
        {
            keyPromptText.gameObject.SetActive(false);
        }
    }

    private void HandleTranscriptDisplay(string text)
    {
        if (transcriptText != null)
        {
            if (hideTranscriptCoroutine != null)
            {
                StopCoroutine(hideTranscriptCoroutine);
            }

            transcriptText.text = text;
            hideTranscriptCoroutine = StartCoroutine(HideTranscriptAfterDelay(6f));
        }
        else
        {
            Debug.LogWarning("UIManager: TranscriptText UI element not set.");
        }
    }

    IEnumerator HideTranscriptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transcriptText.text = "";
    }

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


    public void HideSpeakPrompt()
    {
        if (speakPrompt != null)
        {
            speakPrompt.gameObject.SetActive(false);
        }
    }
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

    public void HideDoorPrompt()
    {
        if (doorPromptText != null)
        {
            doorPromptText.gameObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Clean up event subscriptions when the UIManager is destroyed
        if (playerVoiceInteraction != null)
        {
            playerVoiceInteraction.OnPlayerSpeech -= HandleTranscriptDisplay;
            playerVoiceInteraction.ShowSpeakPrompt -= ShowSpeakPrompt;
            playerVoiceInteraction.HideSpeakPrompt -= HideSpeakPrompt;
        }
    }
}
