using System.Collections;
using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public AudioClip[] clips;
    public float minDelay = 5f;
    public float maxDelay = 15f;

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

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
