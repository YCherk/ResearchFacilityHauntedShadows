using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechDisplay : MonoBehaviour
{
    public Text speechText;
    public PlayerVoiceInteraction playerVoiceInteraction;
    public NPCResponse npcResponse;
    private string currentText = "";

    void Start()
    {
        playerVoiceInteraction.OnPlayerSpeech += DisplaySpeech;
        npcResponse.OnNPCResponse += DisplaySpeech;
    }

    private void DisplaySpeech(string text)
    {
        StopAllCoroutines();
        StartCoroutine(TypeSentence(text));
    }

    IEnumerator TypeSentence(string sentence)
    {
        currentText = "";
        foreach (char letter in sentence.ToCharArray())
        {
            currentText += letter;
            speechText.text = currentText;
            yield return null;
        }
    }

    void OnDestroy()
    {
        playerVoiceInteraction.OnPlayerSpeech -= DisplaySpeech;
        npcResponse.OnNPCResponse -= DisplaySpeech;
    }
}
