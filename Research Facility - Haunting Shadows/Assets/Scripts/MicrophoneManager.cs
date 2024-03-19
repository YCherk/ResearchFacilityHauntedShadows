using UnityEngine;
using UnityEngine.Windows.Speech; // Necessary for speech recognition features
using System;
using System.Collections;

public class MicrophoneManager : MonoBehaviour
{
   
    public static MicrophoneManager Instance;

    // The DictationRecognizer is responsible for speech recognition
    private DictationRecognizer dictationRecognizer;
    // Event fired when the player speaks and the speech is recognized
    public Action<string> OnPlayerSpeech;

    // Frequency at which the status of the DictationRecognizer is checked (in seconds)
    private const float DictationRecognizerCheckInterval = 5f;

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene loads
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }
    }

    void Start()
    {
        // Initializes and starts the DictationRecognizer
        StartDictationRecognizer();
        // Starts a coroutine to periodically check the status of the DictationRecognizer
        StartCoroutine(CheckDictationRecognizerStatusRoutine());
    }

    void OnDestroy()
    {
        // Clean up resources by stopping and disposing of the DictationRecognizer
        StopAndDisposeDictationRecognizer();
        // Ensures all coroutines are terminated when the instance is destroyed
        StopAllCoroutines();
    }

    void StartDictationRecognizer()
    {
        // If an instance already exists, stop and dispose of it before creating a new one
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }

        // Create a new DictationRecognizer and subscribe to its events
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult; // Event for successful dictation
        dictationRecognizer.DictationError += OnDictationError; // Event for dictation errors

        // Start the DictationRecognizer to begin listening for speech
        dictationRecognizer.Start();
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        // Logs the recognized speech and invokes the OnPlayerSpeech event
        Debug.Log("Player said: " + text);
        OnPlayerSpeech?.Invoke(text);
    }

    private void OnDictationError(string error, int hresult)
    {
        // Logs errors from the DictationRecognizer and attempts to restart it
        Debug.LogError("Dictation error: " + error);
        RestartDictationRecognizer(); // Automatically restart the DictationRecognizer on error
    }

    private void StopAndDisposeDictationRecognizer()
    {
        // Safely stops and disposes of the DictationRecognizer if it exists
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }
    }

    private void RestartDictationRecognizer()
    {
        // Helper method to restart the DictationRecognizer by stopping and disposing of the current instance, then starting a new one
        StopAndDisposeDictationRecognizer();
        StartDictationRecognizer();
    }

    IEnumerator CheckDictationRecognizerStatusRoutine()
    {
        // Periodically checks if the DictationRecognizer has stopped for any reason and restarts it if necessary
        while (true)
        {
            yield return new WaitForSeconds(DictationRecognizerCheckInterval);
            if (dictationRecognizer == null || dictationRecognizer.Status != SpeechSystemStatus.Running)
            {
                RestartDictationRecognizer();
            }
        }
    }
}
