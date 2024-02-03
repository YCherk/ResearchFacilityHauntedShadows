using UnityEngine;
using UnityEngine.Windows.Speech;
using System;
using System.Collections;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance;

    private DictationRecognizer dictationRecognizer;
    public Action<string> OnPlayerSpeech;

    // Interval to check DictationRecognizer's status (in seconds)
    private const float DictationRecognizerCheckInterval = 5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartDictationRecognizer();
        StartCoroutine(CheckDictationRecognizerStatusRoutine());
    }

    void OnDestroy()
    {
        StopAndDisposeDictationRecognizer();
        StopAllCoroutines(); // Ensure that all coroutines are stopped when the object is destroyed
    }

    void StartDictationRecognizer()
    {
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }

        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.DictationError += OnDictationError;

        dictationRecognizer.Start();
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("Player said: " + text);
        OnPlayerSpeech?.Invoke(text);
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError("Dictation error: " + error);
        RestartDictationRecognizer(); // Automatically attempt to restart on error
    }

    private void StopAndDisposeDictationRecognizer()
    {
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
        StopAndDisposeDictationRecognizer();
        StartDictationRecognizer();
    }

    IEnumerator CheckDictationRecognizerStatusRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(DictationRecognizerCheckInterval);
            // Check if the DictationRecognizer is not running and restart it
            if (dictationRecognizer == null || dictationRecognizer.Status != SpeechSystemStatus.Running)
            {
                RestartDictationRecognizer();
            }
        }
    }
}
