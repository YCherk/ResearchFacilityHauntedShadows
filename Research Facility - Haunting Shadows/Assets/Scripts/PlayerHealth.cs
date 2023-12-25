using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthBarSlider; // Assign this in the inspector
    public RawImage bloodSplatterRawImage; // Assign this in the inspector
    public Image screenTintPanel; // Assign a full-screen Panel in the inspector
    public ScreenFader screenFader;
    public AudioClip[] damageAudioClips;
    private int currentAudioClipIndex = 0;

    public float splatterDuration = 0.2f; // Short duration for blood splatter
    public float pulseDuration = 1f; // Duration for each pulse
    public float maxPulseAlpha = 0.4f; // Maximum alpha value for pulsing (adjustable in inspector)
    public int healthThresholdForPulse = 30; // Health threshold below which screen will pulse
    public float tintIntensity = 0.5f; // Control the intensity of the screen tint
    public AudioSource damageAudioSource;
    public AudioSource stabSound;

    private bool isPulsing = false; // Track pulsing status

    void Start()
    {
        currentHealth = maxHealth;
        InitializeHealthBar();
        bloodSplatterRawImage.color = new Color(1, 1, 1, 0); // Ensure blood splatter is invisible at start
        screenTintPanel.enabled = false; // Start with the screen tint panel disabled
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        if(damageAudioSource != null && damageAudioClips.Length > 0)
        {
            damageAudioSource.clip = damageAudioClips[currentAudioClipIndex];
            damageAudioSource.Play();

            // Move to the next clip, looping back to the first if necessary
            currentAudioClipIndex = (currentAudioClipIndex + 1) % damageAudioClips.Length;
        }
        
            if (stabSound != null && !stabSound.isPlaying)
            {
                stabSound.Play();
            }
        


        // Ensure the screen tint panel is enabled
        if (screenTintPanel != null && !screenTintPanel.enabled)
        {
            screenTintPanel.enabled = true;
        }

        UpdateScreenTintAndSplatter();

        StopCoroutine("ShowBloodSplatter");
        StartCoroutine(ShowBloodSplatter());

        if (currentHealth <= healthThresholdForPulse && !isPulsing)
        {
            StartCoroutine(PulseScreenRed());
        }
        else if (currentHealth > healthThresholdForPulse && isPulsing)
        {
            StopCoroutine("PulseScreenRed");
            isPulsing = false;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();

        // Disable the screen tint panel completely
        if (screenTintPanel != null)
        {
            screenTintPanel.enabled = false;
        }

        if (currentHealth > healthThresholdForPulse && isPulsing)
        {
            StopCoroutine("PulseScreenRed");
            isPulsing = false;
        }
    }

    private void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }
    }

    private void UpdateScreenTintAndSplatter()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        float alpha = (1 - healthPercent) * tintIntensity; // Use tintIntensity for controlling the screen tint
        if (screenTintPanel.enabled)
        {
            screenTintPanel.color = new Color(1, 0, 0, alpha);
        }
        bloodSplatterRawImage.color = new Color(1, 1, 1, alpha);
    }

    IEnumerator ShowBloodSplatter()
    {
        bloodSplatterRawImage.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(splatterDuration);
        bloodSplatterRawImage.color = new Color(1, 1, 1, 0);
    }

    IEnumerator PulseScreenRed()
    {
        isPulsing = true;
        while (isPulsing)
        {
            float startAlpha = screenTintPanel.color.a;
            for (float t = 0; t <= pulseDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / pulseDuration;
                screenTintPanel.color = new Color(1, 0, 0, Mathf.Lerp(startAlpha, maxPulseAlpha, normalizedTime));
                yield return null;
            }

            for (float t = 0; t <= pulseDuration; t += Time.deltaTime)
            {
                float normalizedTime = t / pulseDuration;
                screenTintPanel.color = new Color(1, 0, 0, Mathf.Lerp(maxPulseAlpha, startAlpha, normalizedTime));
                yield return null;
            }
        }
    }

    private void Die()
    {
        StopCoroutine("PulseScreenRed");
        isPulsing = false;
        screenFader.FadeToGameover();
    }
}
