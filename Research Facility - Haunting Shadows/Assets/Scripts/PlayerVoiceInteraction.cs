using UnityEngine;
using System;

public class PlayerVoiceInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    public Action<string> OnPlayerSpeech;
    public Action ShowSpeakPrompt;
    public Action HideSpeakPrompt;

    void Start()
    {
        // Subscribe to the OnPlayerSpeech event from the MicrophoneManager
        MicrophoneManager.Instance.OnPlayerSpeech += OnPlayerSpeechDetected;

        // Setup the actions to show and hide the speak prompt via UIManager
        ShowSpeakPrompt += UIManager.Instance.ShowSpeakPrompt;
        HideSpeakPrompt += UIManager.Instance.HideSpeakPrompt;

    }

    private void OnPlayerSpeechDetected(string text)
    {
        Debug.Log("Player said: " + text);
        OnPlayerSpeech?.Invoke(text);

        // Process the speech and trigger NPC responses based on certain keywords
        ProcessSpeechAndTriggerResponses(text.ToLower());
    }

    private void ProcessSpeechAndTriggerResponses(string spokenText)
    {
        if (isPlayerInRange)
        {
            if (spokenText.Contains("hello"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer();
            }
            else if (spokenText.Contains("who are you"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer1();
            }
            else if (spokenText.Contains("escape"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer2();
            }
            else if (spokenText.Contains("help me"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer3();
            }
            else if (spokenText.Contains("key"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer4();
            }
            else if (spokenText.Contains("story"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer5();
            }
            else if (spokenText.Contains("good"))
            {
                FindObjectOfType<NPCResponse>().RespondToPlayer6();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            isPlayerInRange = true;
            ShowSpeakPrompt?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("NPC"))
        {
            isPlayerInRange = false;
            HideSpeakPrompt?.Invoke();
        }
    }

    void OnDestroy()
    {
        // Clean up by unsubscribing from the MicrophoneManager's event when this object is destroyed
        if (MicrophoneManager.Instance != null)
        {
            MicrophoneManager.Instance.OnPlayerSpeech -= OnPlayerSpeechDetected;
        }
    }
}
