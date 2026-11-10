
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyStatusEffects : MonoBehaviour
{
    private NavMeshAgent agent;

    private bool isSlowed = false;
    private float normalSpeed;
    private float appliedSpeed;
    private float slowMultiplier = 1f;
    private float slowEndTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Slow an enemy for a certain number of seconds.
    public void ApplySlow(float multiplier, float duration)
    {
        if (agent == null || !agent.isActiveAndEnabled)
            return;

        if (!isSlowed)
        {
            normalSpeed = agent.speed;
            isSlowed = true;
            appliedSpeed = agent.speed;
        }

        slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowEndTime = Time.time + duration;
    }

    private void LateUpdate()
    {
        if (!isSlowed || agent == null ||
            !agent.isActiveAndEnabled)
            return;

        // Restore speed once the effect ends.
        if (Time.time >= slowEndTime)
        {
            agent.speed = normalSpeed;
            isSlowed = false;
            return;
        }

        // Some enemy AI scripts change their movement speed.
        // Track those changes so Imp rushes also get slowed.
        if (Mathf.Abs(agent.speed - appliedSpeed) > 0.01f)
        {
            normalSpeed = agent.speed;
        }

        appliedSpeed = normalSpeed * slowMultiplier;
        agent.speed = appliedSpeed;
    }

    // Push an enemy away from a position.
    public void Knockback(Vector3 source, float distance)
    {
        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
            return;

        Vector3 direction = transform.position - source;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        Vector3 start = agent.nextPosition;
        Vector3 target = start + direction * distance;

        // Don't push through NavMesh obstacles.
        NavMeshHit wallHit;

        if (NavMesh.Raycast(
            start, target, out wallHit, agent.areaMask))
        {
            target = Vector3.Lerp(
                start, wallHit.position, 0.8f);
        }

        // Find a valid destination on the NavMesh.
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(
            target, out navHit, 0.5f, agent.areaMask))
        {
            // Small instant shove using the navigation agent.
            agent.Warp(navHit.position);
        }
    }

    private void OnDisable()
    {
        if (isSlowed && agent != null)
        {
            agent.speed = normalSpeed;
            isSlowed = false;
        }
    }
}
