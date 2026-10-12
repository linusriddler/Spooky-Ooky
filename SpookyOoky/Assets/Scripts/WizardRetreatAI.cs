

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WizardRetreatAI : MonoBehaviour
{
    [Header("Approach Settings")]
    public float approachDistance = 12f;
    public float stopApproachingDistance = 10f;
    public float approachSpeed = 2.5f;

    [Header("Retreat Settings")]
    public float retreatDistance = 6f;
    public float safeDistance = 8f;
    public float retreatSpeed = 3f;

    [Header("Pathfinding")]
    public float retreatStep = 4f;
    public float pathUpdateInterval = 0.5f;

    private Transform player;
    private NavMeshAgent agent;

    private float pathTimer;

    private bool isRetreating = false;
    private bool isApproaching = false;

    private void Start()
    {
        // Find the player.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        agent = GetComponent<NavMeshAgent>();

        // Let WizardRangedAttack control rotation.
        agent.updateRotation = false;
        agent.stoppingDistance = 0.2f;
    }

    private void Update()
    {
        if (player == null || !agent.isOnNavMesh)
            return;

        // Calculate horizontal distance to the player.
        Vector3 difference =
            transform.position - player.position;

        difference.y = 0f;

        float distance = difference.magnitude;

        // PRIORITY 1: RETREAT
        // Retreating is more important than approaching.
        if (distance < retreatDistance)
        {
            if (!isRetreating)
            {
                isRetreating = true;
                isApproaching = false;
                pathTimer = 0f;
            }
        }

        // Stop retreating once we're far enough away.
        if (isRetreating && distance >= safeDistance)
        {
            isRetreating = false;
            agent.ResetPath();
        }

        // Handle retreat movement.
        if (isRetreating)
        {
            agent.speed = retreatSpeed;

            pathTimer -= Time.deltaTime;

            if (pathTimer <= 0f)
            {
                FindRetreatPosition();
                pathTimer = pathUpdateInterval;
            }

            return;
        }

        // PRIORITY 2: APPROACH
        // Start approaching if the player is too far away.
        if (distance > approachDistance)
        {
            isApproaching = true;
        }

        // Stop approaching once we're close enough.
        if (isApproaching &&
            distance <= stopApproachingDistance)
        {
            isApproaching = false;
            agent.ResetPath();
        }

        // Handle approach movement.
        if (isApproaching)
        {
            agent.speed = approachSpeed;

            pathTimer -= Time.deltaTime;

            if (pathTimer <= 0f)
            {
                agent.SetDestination(player.position);
                pathTimer = pathUpdateInterval;
            }

            return;
        }

        // PRIORITY 3: STAND AND ATTACK
        // We're at a comfortable distance.
        if (agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    private void FindRetreatPosition()
    {
        Vector3 awayDirection =
            transform.position - player.position;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.001f)
            awayDirection = -transform.forward;

        awayDirection.Normalize();

        // Try different directions to avoid walls.
        float[] angles = { 0f, 35f, -35f, 70f, -70f };

        Vector3 bestPosition = transform.position;

        float bestDistance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool foundPosition = false;

        foreach (float angle in angles)
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f)
                * awayDirection;

            Vector3 targetPosition =
                transform.position +
                direction * retreatStep;

            NavMeshHit hit;

            if (!NavMesh.SamplePosition(
                targetPosition,
                out hit,
                1.5f,
                NavMesh.AllAreas))
            {
                continue;
            }

            // Check whether the destination is reachable.
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            float newDistance = Vector3.Distance(
                hit.position,
                player.position
            );

            if (newDistance > bestDistance + 0.1f)
            {
                bestDistance = newDistance;
                bestPosition = hit.position;
                foundPosition = true;
            }
        }

        // Move toward the best retreat location.
        if (foundPosition)
        {
            agent.SetDestination(bestPosition);
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void OnDisable()
    {
        // Stop moving when the Wizard dies.
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}

