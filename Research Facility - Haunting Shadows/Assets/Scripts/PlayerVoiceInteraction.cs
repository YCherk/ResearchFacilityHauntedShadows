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
        MicrophoneManager.Instance.OnPlayerSpeech += OnPlayerSpeechDetected;
    }

    private void OnPlayerSpeechDetected(string text)
    {
        Debug.Log("Player said: " + text);
        OnPlayerSpeech?.Invoke(text);

        if (isPlayerInRange && text.ToLower().Contains("hello"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer();
        }
        if (isPlayerInRange && text.ToLower().Contains("who are you"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer1();
        }
        if (isPlayerInRange && text.ToLower().Contains("escape"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer2();
        }
        if (isPlayerInRange && text.ToLower().Contains("help me"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer3();
        }
        if (isPlayerInRange && text.ToLower().Contains("key"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer4();
        }
        if (isPlayerInRange && text.ToLower().Contains("story"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer5();
        }
        if (isPlayerInRange && text.ToLower().Contains("good"))
        {
            FindObjectOfType<NPCResponse>().RespondToPlayer6();
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
        if (MicrophoneManager.Instance != null)
        {
            MicrophoneManager.Instance.OnPlayerSpeech -= OnPlayerSpeechDetected;
        }
    }
}
