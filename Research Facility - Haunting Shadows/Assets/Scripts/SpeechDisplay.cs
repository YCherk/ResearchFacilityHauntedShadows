using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpeechDisplay : MonoBehaviour
{
    public Text speechText;
    public PlayerVoiceInteraction playerVoiceInteraction;
    public NPCResponse npcResponse;
    public NPCResponse npcResponse1;
    private string currentText = "";

    void Start()
    {
        playerVoiceInteraction.OnPlayerSpeech += DisplaySpeech;
        npcResponse.OnNPCResponse += DisplaySpeech;
        npcResponse1.OnNPCResponse += DisplaySpeech;
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

        // Start coroutine to clear text after 4 seconds
        StartCoroutine(ClearTextAfterDelay(4f));
    }

    IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        speechText.text = ""; // Clear the text
    }

    void OnDestroy()
    {
        playerVoiceInteraction.OnPlayerSpeech -= DisplaySpeech;
        npcResponse.OnNPCResponse -= DisplaySpeech;
        npcResponse1.OnNPCResponse -= DisplaySpeech;
    }
}
