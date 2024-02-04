using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public float timeToOpenDoor = 10f; // Time the enemy will wait before opening the door
    private float doorTimer = 0f; // Timer to track the waiting time

    private float timeSinceLastSeen = 0f; // Time since the player was last seen
    public float timeToGiveUpChase = 5f; // Time after which enemy gives up the chase

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

    public Text warningText;
    public CanvasGroup warningTextCanvasGroup;
    public float fadeDuration = 0.5f;

    public GameObject jumpscareCanvas;
    private float chaseDuration = 0f;
    private bool isJumpscareTriggered = false;
    public AudioSource jumpScare;
    public CanvasGroup jumpscareCanvasGroup;
    public float jumpscareChance = 0.3f;
    private Coroutine jumpscareCoroutine = null;

    public AudioSource walkingAudioSource;
    public AudioSource runningAudioSource;
    public AudioSource attackAudioSource;
    public AudioSource knockOnDoor;

    public AudioClip[] audioClips;
    public float audioPlayChance = 0.5f;
    private bool isAudioPlaying = false;

    public float sensitivity = 100;
    public float loudnessThreshold = 10;
    public Slider volumeSlider;
    private AudioClip microphoneInput;
    private bool isMicrophoneInitialized = false;
    public int sampleWindow = 64;
    public Text dialogueScreamText;

    private float timer;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isChasingPlayer = false;
    private bool isSearchingForPlayer = false;
    private Vector3 lastKnownPlayerPosition;

    public float minFlickerDuration = 0.1f;
    public float maxFlickerDuration = 0.5f;
    private Coroutine flickerCoroutine = null;
  

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        timer = patrolTimer;
        agent.speed = moveSpeed;
        agent.updateRotation = true;
        InitializeMicrophone();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);
        bool canSeePlayerFromBehind = CanSeePlayerFromBehind(distanceToPlayer);

        animator.SetBool("WalkForward", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("IsAttack", false);
        animator.SetBool("isIdle", false);

        if (isMicrophoneInitialized)
        {
            int micPosition = Microphone.GetPosition(null) - sampleWindow;
            if (micPosition < 0) return;

            float micLoudness = GetAveragedVolume(micPosition, microphoneInput) * sensitivity;
            volumeSlider.value = Mathf.Clamp(micLoudness, 0, volumeSlider.maxValue);

            if (micLoudness > loudnessThreshold)
            {
                StartCoroutine(DisplayWarning());
                TurnTowardsPlayer();
                isChasingPlayer = true;
                isSearchingForPlayer = false;
                lastKnownPlayerPosition = player.position;
                agent.SetDestination(player.position);
                animator.SetBool("isRunning", true);
                PlayRandomAudioClip();
            }
        }

        if (canSeePlayer || canSeePlayerFromBehind)
        {
            TurnTowardsPlayer();
            isChasingPlayer = true;
            isSearchingForPlayer = false;
            lastKnownPlayerPosition = player.position;
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);
            ManageFlickering();

            chaseDuration += Time.deltaTime;

            if (chaseDuration > 15f && !isJumpscareTriggered && Random.value < jumpscareChance)
            {
                isJumpscareTriggered = true;
                jumpscareCoroutine = StartCoroutine(TriggerJumpscare());
            }

            if (distanceToPlayer <= attackDistance)
            {
                animator.SetBool("IsAttack", true);
                PlayRandomAudioClip();
            }
            else
            {
                animator.SetBool("IsAttack", false);
            }
        }
        else if (isChasingPlayer)
        {
            // If LOS is broken, switch to searching
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen > timeToGiveUpChase)
            {
                isChasingPlayer = false;
                isSearchingForPlayer = true;
                StopFlickering();
                lastKnownPlayerPosition = player.position;
                agent.SetDestination(lastKnownPlayerPosition);
            }
        }
        else if (isSearchingForPlayer)
        {
            doorTimer += Time.deltaTime;
            if (doorTimer >= timeToOpenDoor)
            {
                CheckAndOpenDoor();
                doorTimer = 0f; // Reset the timer
            }

            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
            {
                isSearchingForPlayer = false;
                Patrol(); // Resume patrol after reaching last known position
            }
            else
            {
                animator.SetBool("isRunning", true);
            }
        }
        else
        {
            // Resume patrol if not chasing or searching
            Patrol();
        }

        HandleAudioPlayback();
    }

    private void CheckAndOpenDoor()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f); // Check within 2 meters
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Door")) // Ensure your door has the tag "Door"
            {
                
                DoorController door = hitCollider.GetComponent<DoorController>();
                if (door != null)
                {
                    knockOnDoor.Play();
                    door.ForceOpenDoor(); // Force the door open
                }
            }
        }
    }
    public void SetDifficultyParameters(float newChaseSpeed, float newSightDistance, float newAttackDistance, float newFieldOfView, float newSensitivity, float newLoudnessThreshold)
    {
        chaseSpeed = newChaseSpeed;
        sightDistance = newSightDistance;
        attackDistance = newAttackDistance;
        fieldOfView = newFieldOfView;
        sensitivity = newSensitivity;
        loudnessThreshold = newLoudnessThreshold;
    }
    private IEnumerator DisplayDialogue(string message, float duration)
    {
        dialogueScreamText.text = message;
        dialogueScreamText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        dialogueScreamText.gameObject.SetActive(false);
    }
    private void ManageFlickering()
    {
        if (flickerCoroutine == null)
        {
            flickerCoroutine = StartCoroutine(Flicker());
        }
    }

    private void StopFlickering()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
            SetEnemyVisibility(true);
        }
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            float flickerTime = Random.Range(minFlickerDuration, maxFlickerDuration);

            SetEnemyVisibility(!IsEnemyVisible());
            agent.speed = IsEnemyVisible() ? chaseSpeed : chaseSpeed * 2;

            yield return new WaitForSeconds(flickerTime);
        }
    }

    private void SetEnemyVisibility(bool isVisible)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = isVisible;
        }
    }

    private bool IsEnemyVisible()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            return renderers[0].enabled;
        }
        return true;
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
        warningText.text = "It heard you"; // Set the text
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
        foreach (var sample in waveData)
        {
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
                    // Check if the raycast directly hits the player
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
        // Check if the enemy has reached its current destination or if there's no path currently being computed
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Only increment the timer if the agent has stopped moving, indicating it has reached the destination
            if (agent.velocity.sqrMagnitude == 0f)
            {
                timer += Time.deltaTime;
            }

            // When the timer exceeds the patrolTimer, find a new patrol point
            if (timer >= patrolTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1);
                agent.SetDestination(newPos);
                timer = 0; // Reset the timer after setting a new destination
            }
        }
        else
        {
            // Reset the timer if the agent is moving towards a destination
            timer = 0;
        }

        // Optionally, adjust the animation based on whether the agent is moving or not
        animator.SetBool("WalkForward", agent.velocity.magnitude > 0.1f);
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
        if (!isChasingPlayer) return; // Only turn towards player if chasing

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}