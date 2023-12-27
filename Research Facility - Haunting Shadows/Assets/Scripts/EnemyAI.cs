using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;
public class EnemyAI : MonoBehaviour
{
    public float patrolRadius = 10.0f;
    public float patrolTimer = 5.0f;
    public float moveSpeed = 2.0f;
    public float chaseSpeed = 4.0f;
    public Transform player;
    public float attackDistance = 2.0f;
    public float sightDistance = 15.0f;
    public float backSightDistance = 5.0f;
    public float fieldOfView = 60.0f;
    public float backFieldOfView = 90.0f;


    public Text warningText; // UI Text for displaying the warning
    public CanvasGroup warningTextCanvasGroup; // CanvasGroup for the warning text
    public float fadeDuration = 0.5f;

    public GameObject jumpscareCanvas; // Assign in the Inspector
    private float chaseDuration = 0f;
    private bool isJumpscareTriggered = false;
    public AudioSource jumpScare;
    public CanvasGroup jumpscareCanvasGroup;
    public float jumpscareChance = 0.3f; // 30% chance to trigger jumpscare
    private Coroutine jumpscareCoroutine = null;



    // Audio sources for different states
    public AudioSource walkingAudioSource;
    public AudioSource runningAudioSource;
    public AudioSource attackAudioSource;

    public AudioClip[] audioClips; // Array of audio clips
    private int playCount = 0; // Counter for tracking the number of times the threshold is exceeded
    private int nextPlayCount = 1; // The count at which the audio clip will be played next
    public float audioPlayChance = 0.5f; // Chance of playing an audio clip (0.5 for 50%)
    private bool isAudioPlaying = false; // To track if an audio clip is currently playing

    // Microphone input and UI variables
    public float sensitivity = 100;
    public float loudnessThreshold = 10;
    public Slider volumeSlider; // Assign this in the Inspector
    private AudioClip microphoneInput;
    private bool isMicrophoneInitialized = false;
    public int sampleWindow = 64;


    private float timer;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasingPlayer = false;
    private bool isSearchingForPlayer = false;
    private Vector3 lastKnownPlayerPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        timer = patrolTimer;
        agent.speed = moveSpeed; // Set initial speed to patrol speed
        agent.updateRotation = false;
        InitializeMicrophone();
      
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);
        bool canSeePlayerFromBehind = CanSeePlayerFromBehind(distanceToPlayer);

        if (isMicrophoneInitialized)
        {
            int micPosition = Microphone.GetPosition(null) - sampleWindow; // Get current position of the mic recording
            if (micPosition < 0) return;

            float micLoudness = GetAveragedVolume(micPosition, microphoneInput) * sensitivity;
            volumeSlider.value = Mathf.Clamp(micLoudness, 0, volumeSlider.maxValue);

            if (micLoudness > loudnessThreshold)
            {
                StartCoroutine(DisplayWarning());
                TurnTowardsPlayer(); // Continuously turn towards the player during the chase
                agent.speed = chaseSpeed; // Increase speed when starting to chase

                playCount++;

                if (playCount >= nextPlayCount && Random.value < audioPlayChance)
                {
                    PlayRandomAudioClip();
                    nextPlayCount = playCount + Random.Range(1, 5); // Randomize the next play count
                }

                isChasingPlayer = true;
                isSearchingForPlayer = false;
                lastKnownPlayerPosition = player.position;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);

                if (distanceToPlayer <= attackDistance)
                {
                    animator.SetTrigger("IsAttack");
                }
                else
                {
                    animator.SetBool("IsAttack", false);
                }
            }
        }
        
        else
        {
            chaseDuration = 0f;
        }

        if (canSeePlayer || canSeePlayerFromBehind)
        {
            TurnTowardsPlayer(); // Continuously turn towards the player during the chase
            agent.speed = chaseSpeed; // Increase speed when starting to chase

            isChasingPlayer = true;
            isSearchingForPlayer = false;
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);

            chaseDuration += Time.deltaTime;

            if (chaseDuration > 15f && !isJumpscareTriggered && Random.value < jumpscareChance)
            {
                isJumpscareTriggered = true; // Set this flag as soon as you decide to trigger the jumpscare
                jumpscareCoroutine = StartCoroutine(TriggerJumpscare());
            }
        }
        else if (isChasingPlayer)
        {
            // Reset logic when the enemy loses sight of the player
            isChasingPlayer = false;
            isSearchingForPlayer = true;
            chaseDuration = 0f;
            isJumpscareTriggered = false; // Ensure this is reset here
            if (jumpscareCoroutine != null)
            {
                StopCoroutine(jumpscareCoroutine);
                jumpscareCoroutine = null;
            }
            agent.speed = moveSpeed; // Reset speed when stopping the chase
            agent.SetDestination(lastKnownPlayerPosition);
        }
        else if (isSearchingForPlayer)
            {
                if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
                {
                    isSearchingForPlayer = false;
                    animator.SetBool("isRunning", false); // Stop running after search
                    Patrol();
                }
                else
                {
                    // Keep running if still searching
                    animator.SetBool("isRunning", true);
                }
            }
            else
            {
                Patrol();
            }

            animator.SetBool("isIdle", agent.velocity.magnitude < 0.01f);
            if (agent.velocity.magnitude > 0.01f && !canSeePlayerFromBehind)
            {
                transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
            }

            // Handle audio based on the enemy state
            HandleAudioPlayback();
        
    }
    IEnumerator TriggerJumpscare()
    {
        isJumpscareTriggered = true;
        jumpScare.Play();
        jumpscareCanvas.SetActive(true);

        // Flash Effect
        float flashDuration = 0.1f; // Duration of each flash
        int numberOfFlashes = 3; // Total number of flashes

        for (int i = 0; i < numberOfFlashes; i++)
        {
            // Flash on
            jumpscareCanvasGroup.alpha = 1;
            yield return new WaitForSeconds(flashDuration);

            // Flash off
            jumpscareCanvasGroup.alpha = 0;
            yield return new WaitForSeconds(flashDuration);
        }

        // Show final jumpscare image for a brief moment
        jumpscareCanvasGroup.alpha = 1;

        jumpscareCanvas.SetActive(false);
        yield return new WaitForSeconds(1f);

        jumpscareCoroutine = null;
    }

    void PlayRandomAudioClip()
    {
        if (audioClips.Length > 0 && !isAudioPlaying)
        {
            int clipIndex = Random.Range(0, audioClips.Length);
            AudioClip clipToPlay = audioClips[clipIndex];
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position); // Play the clip at the enemy's position
            isAudioPlaying = true;

            // Wait for the length of the clip before allowing another clip to be played
            StartCoroutine(ResetAudioPlayingFlag(clipToPlay.length));
        }
    }

    IEnumerator ResetAudioPlayingFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAudioPlaying = false;
    }

    IEnumerator DisplayWarning()
    {
        warningText.text = "You Screamed. Run."; // Set the text
        warningText.gameObject.SetActive(true); // Show warning text

        // Fade in
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            warningTextCanvasGroup.alpha = elapsedTime / fadeDuration;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        warningTextCanvasGroup.alpha = 1;

        yield return new WaitForSeconds(2); // Wait for 2 seconds

        // Fade out
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            warningTextCanvasGroup.alpha = 1 - (elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        warningTextCanvasGroup.alpha = 0;

        warningText.gameObject.SetActive(false); // Hide warning text
    }
    void InitializeMicrophone()
    {
        int sampleRate = 44100; // Standard sampling rate
        int recordingLength = 10; // Length of the recording in seconds
        microphoneInput = Microphone.Start(null, true, recordingLength, sampleRate);
        isMicrophoneInitialized = true;
    }


    public float GetAveragedVolume(int clipPosition, AudioClip clip)
    {
        int startPosition = clipPosition - sampleWindow;

        if (startPosition < 0) return 0;

        float[] waveData = new float[sampleWindow];

        clip.GetData(waveData, startPosition);

        float totalLoudness = 0;
        foreach (var sample in waveData) {
            totalLoudness += Mathf.Abs(sample);
        }

        return totalLoudness / sampleWindow;
    }


    void HandleAudioPlayback()
    {
        // Walking audio
        if (IsWalking() && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Play();
            runningAudioSource.Stop();
            attackAudioSource.Stop();
        }
        // Running audio
        else if (isChasingPlayer && !runningAudioSource.isPlaying)
        {
            runningAudioSource.Play();
            walkingAudioSource.Stop();
            attackAudioSource.Stop();
        }
        
        
        // Stop audio if idle
        else if (IsIdle())
        {
            walkingAudioSource.Stop();
            runningAudioSource.Stop();
            attackAudioSource.Stop();
        }
    }

    bool IsWalking()
    {
        return !isChasingPlayer && !isSearchingForPlayer && agent.velocity.magnitude > 0.01f;
    }

    public void PlayAttackSound()
    {
        if (attackAudioSource != null && !attackAudioSource.isPlaying)
        {
            attackAudioSource.Play();
        }
    }

   

    bool IsIdle()
    {
        return agent.velocity.magnitude < 0.01f;
    }

    public void AttackPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(30);
        }
    }

    bool CanSeePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer <= sightDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle <= fieldOfView / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, sightDistance))
                {
                    return hit.transform == player;
                }
            }
        }
        return false;
    }

    bool CanSeePlayerFromBehind(float distanceToPlayer)
    {
        if (distanceToPlayer <= backSightDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(-transform.forward, directionToPlayer);
            if (angle <= backFieldOfView / 2)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, backSightDistance))
                {
                    return hit.transform == player;
                }
            }
        }
        return false;
    }

    void Patrol()
    {
        timer += Time.deltaTime;
        animator.SetBool("WalkForward", true);

        if (timer >= patrolTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        if (agent.velocity.magnitude > 0.01f && !walkingAudioSource.isPlaying)
        {
            walkingAudioSource.Play();
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = origin + randDirection;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPos, out navHit, dist, layermask))
            {
                return navHit.position;
            }
        }
        return origin;
    }

    private void TurnTowardsPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }
}
