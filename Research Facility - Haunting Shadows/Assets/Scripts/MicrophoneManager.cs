using UnityEngine;
using UnityEngine.Windows.Speech;
using System;
using System.Linq;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager Instance;

    private DictationRecognizer dictationRecognizer;
    public Action<string> OnPlayerSpeech;
    public Action<float> OnMicLoudness;

    private AudioClip microphoneInput;
    private bool isMicrophoneInitialized = false;
    public int sampleWindow = 64;
    

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

    }

    void Update()
    {
        
    }

    void StartDictationRecognizer()
    {
        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationResult += OnDictationResult;
        dictationRecognizer.Start();
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        Debug.Log("Player said: " + text);
        OnPlayerSpeech?.Invoke(text);
    }

    void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.DictationResult -= OnDictationResult;
            dictationRecognizer.Dispose();
        }

        if (isMicrophoneInitialized)
        {
            Microphone.End(null);
        }
    }
}
