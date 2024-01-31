using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text collectText;
    public Text transcriptText; // Text element for displaying spoken words
    public Text speakPrompt; // Text element for the "Speak to Spirit" prompt
    public PlayerVoiceInteraction playerVoiceInteraction; // Reference to PlayerVoiceInteraction script

    private Coroutine hideTranscriptCoroutine;

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
    }

    private void Start()
    {
        // Subscribe to events from the PlayerVoiceInteraction script
        playerVoiceInteraction.OnPlayerSpeech += HandleTranscriptDisplay;
        playerVoiceInteraction.ShowSpeakPrompt += ShowSpeakPrompt;
        playerVoiceInteraction.HideSpeakPrompt += HideSpeakPrompt;
    }

    public void ShowMessage(string message, float duration)
    {
        StartCoroutine(DisplayMessage(message, duration));
    }

    private IEnumerator DisplayMessage(string message, float duration)
    {
        collectText.text = message;
        collectText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        collectText.gameObject.SetActive(false);
    }

    private void HandleTranscriptDisplay(string text)
    {
        if (hideTranscriptCoroutine != null)
        {
            StopCoroutine(hideTranscriptCoroutine);
        }

        transcriptText.text = text;
        hideTranscriptCoroutine = StartCoroutine(HideTranscriptAfterDelay(6f)); // Hide after 4 seconds
    }

    IEnumerator HideTranscriptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        transcriptText.text = "";
    }

    private void ShowSpeakPrompt()
    {
        speakPrompt.gameObject.SetActive(true);
    }

    private void HideSpeakPrompt()
    {
        speakPrompt.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (playerVoiceInteraction != null)
        {
            playerVoiceInteraction.OnPlayerSpeech -= HandleTranscriptDisplay;
            playerVoiceInteraction.ShowSpeakPrompt -= ShowSpeakPrompt;
            playerVoiceInteraction.HideSpeakPrompt -= HideSpeakPrompt;
        }
    }
}
