using System.Collections;
using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    public float minDelay = 5f;
    public float maxDelay = 15f;
    public float playChance = 0.3f; // 30% chance to play the audio

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayRandomClip());
    }

    IEnumerator PlayRandomClip()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // Decide whether to play the clip based on playChance
            if (Random.value <= playChance)
            {
                // Ensure there's at least one clip to play
                if (clips.Length > 0)
                {
                    AudioClip clip = clips[Random.Range(0, clips.Length)];
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }
        }
    }
}
