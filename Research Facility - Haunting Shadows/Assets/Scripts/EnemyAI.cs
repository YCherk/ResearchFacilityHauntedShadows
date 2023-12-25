using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float patrolRadius = 10.0f;
    public float patrolTimer = 5.0f;
    public float moveSpeed = 2.0f;
    public float chaseSpeed = 4.0f; // Speed when chasing the player
    public Transform player;
    public float attackDistance = 2.0f;
    public float sightDistance = 15.0f;
    public float backSightDistance = 5.0f;
    public float fieldOfView = 60.0f;
    public float backFieldOfView = 90.0f;
    public AudioSource walkingAudio;

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
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);
        bool canSeePlayerFromBehind = CanSeePlayerFromBehind(distanceToPlayer);

        if (canSeePlayer || canSeePlayerFromBehind)
        {
            if (!isChasingPlayer)
            {
                TurnTowardsPlayer();
                agent.speed = chaseSpeed; // Increase speed when starting to chase
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
        else if (isChasingPlayer)
        {
            isChasingPlayer = false;
            isSearchingForPlayer = true;
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

        if (timer >= patrolTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, patrolRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }

        animator.SetBool("WalkForward", agent.velocity.magnitude > 0.01f);
        walkingAudio.Play();
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
