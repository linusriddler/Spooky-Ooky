
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class ImpCombatAI : MonoBehaviour
{
    [Header("Movement")]
    public float approachSpeed = 3.7f;
    public float rushSpeed = 8.5f;
    public float attackDistance = 10f;
    public float rotationSpeed = 8f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject chargeEffect;
    public float projectileSpeed = 9f;
    public float projectileDamage = 9f;
    public float castingTime = 0.65f;

    [Header("Melee Rush")]
    public Transform meleePoint;
    public GameObject slashEffectPrefab;
    public float meleeDamage = 12f;
    public float meleeRange = 2f;
    public float meleeHitRadius = 1.3f;
    public float maxRushDuration = 1.5f;

    [Header("Retreat")]
    public float retreatStep = 5f;
    public float retreatDuration = 1.5f;

    [Header("Teleport Settings")]
    public float teleportCooldown = 6f;
    public float minTeleportDistance = 2f;
    public float maxTeleportDistance = 7f;
    public float minDistanceFromPlayer = 3f;

    public GameObject teleportEffectPrefab;

    private int lastTeleportIndex = -1;

    private ImpTeleportPoints teleportPoints;
    private float nextTeleportTime;

    [Header("Attack Timing")]
    public float timeBetweenAttacks = 1.5f;

    private Transform player;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // The script controls facing direction.
        agent.updateRotation = false;
        agent.stoppingDistance = 1.2f;

        // Find the player.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        if (chargeEffect != null)
            chargeEffect.SetActive(false);

        // Find the teleport locations in the current scene.
        teleportPoints = FindFirstObjectByType<ImpTeleportPoints>();

        // Start the teleport cooldown.
        nextTeleportTime = Time.time + teleportCooldown;

        StartCoroutine(CombatLoop());
    }

    private void Update()
    {
        if (player == null)
            return;

        // Rotate toward the player horizontally.
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Controls the Imp's overall attack pattern.
    private IEnumerator CombatLoop()
    {
        bool useRangedAttack = true;

        while (true)
        {
            if (player == null || !agent.isOnNavMesh)
            {
                yield return null;
                continue;
            }

            float distance = HorizontalDistanceToPlayer();

            // Approach until we are within attack distance.
            if (distance > attackDistance)
            {
                agent.speed = approachSpeed;
                agent.SetDestination(player.position);

                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // Stop before beginning an attack.
            agent.ResetPath();

            if (useRangedAttack)
            {
                // Charge and fire a projectile.
                yield return StartCoroutine(RangedAttack());
            }
            else
            {
                // Rush toward the player and then retreat.
                yield return StartCoroutine(RushAttack());
            }

            // Alternate between ranged and melee attacks.
            useRangedAttack = !useRangedAttack;

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private IEnumerator RangedAttack()
    {
        // Display charging effect.
        if (chargeEffect != null)
            chargeEffect.SetActive(true);

        yield return new WaitForSeconds(castingTime);

        if (chargeEffect != null)
            chargeEffect.SetActive(false);

        if (player == null ||
            projectilePrefab == null ||
            firePoint == null)
            yield break;

        // Aim toward the player's center.
        Vector3 targetPosition =
            player.position + Vector3.up;

        Vector3 direction =
            (targetPosition - firePoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        // Prevent the projectile from damaging its owner.
        WizardProjectile projectileScript =
            projectile.GetComponent<WizardProjectile>();

        if (projectileScript != null)
        {
            projectileScript.SetOwner(transform);
            projectileScript.damage = projectileDamage;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
    }

    private IEnumerator RushAttack()
    {
        if (player == null)
            yield break;

        // Start a fast rush toward the player.
        agent.speed = rushSpeed;

        float timer = 0f;
        float pathTimer = 0f;

        while (timer < maxRushDuration)
        {
            if (player == null || !agent.isOnNavMesh)
                yield break;

            // Stop rushing once we're close enough.
            if (HorizontalDistanceToPlayer() <= meleeRange)
                break;

            // Refresh the destination every 0.15 seconds.
            pathTimer -= Time.deltaTime;

            if (pathTimer <= 0f)
            {
                agent.SetDestination(player.position);
                pathTimer = 0.15f;
            }

            timer += Time.deltaTime;

            yield return null;
        }

        agent.ResetPath();

        // Attack only if we reached the player.
        if (HorizontalDistanceToPlayer() <= meleeRange)
        {
            PerformMeleeAttack();
        }

        // Brief pause after rushing.
        yield return new WaitForSeconds(0.25f);

        // Escape after the rush.
        yield return StartCoroutine(Retreat());

        // Occasionally teleport after retreating.
        if (Time.time >= nextTeleportTime)
        {
            TryTeleport();
        }
    }

    private void PerformMeleeAttack()
    {
        if (meleePoint == null)
            return;

        // Spawn the visual slash effect.
        if (slashEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                slashEffectPrefab,
                meleePoint.position,
                transform.rotation
            );

            Destroy(effect, 1f);
        }

        // Check for the player near the melee point.
        Collider[] hits = Physics.OverlapSphere(
            meleePoint.position,
            meleeHitRadius
        );

        foreach (Collider hit in hits)
        {
            PlayerHealth health =
                hit.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(meleeDamage);

                // Only damage the player once per attack.
                break;
            }
        }
    }

    private IEnumerator Retreat()
    {
        if (player == null || !agent.isOnNavMesh)
            yield break;

        Vector3 awayDirection =
            transform.position - player.position;

        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.001f)
            awayDirection = -transform.forward;

        awayDirection.Normalize();

        // Try several escape directions.
        float[] angles = { 0f, 40f, -40f, 80f, -80f };

        Vector3 bestPosition = transform.position;
        float bestDistance = HorizontalDistanceToPlayer();
        bool found = false;

        foreach (float angle in angles)
        {
            Vector3 direction =
                Quaternion.Euler(0f, angle, 0f)
                * awayDirection;

            Vector3 target =
                transform.position + direction * retreatStep;

            NavMeshHit hit;

            if (!NavMesh.SamplePosition(
                target,
                out hit,
                1.5f,
                NavMesh.AllAreas))
                continue;

            // Only choose reachable locations.
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            Vector3 difference =
                hit.position - player.position;
            difference.y = 0f;

            float distance = difference.magnitude;

            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestPosition = hit.position;
                found = true;
            }
        }

        if (found)
        {
            agent.speed = rushSpeed;
            agent.SetDestination(bestPosition);

            // Allow time to escape.
            yield return new WaitForSeconds(retreatDuration);
        }

        agent.ResetPath();
    }

    private float HorizontalDistanceToPlayer()
    {
        if (player == null)
            return Mathf.Infinity;

        Vector3 difference =
            transform.position - player.position;

        difference.y = 0f;

        return difference.magnitude;
    }

    private void OnDisable()
    {
        // Cancel ongoing attacks if the Imp dies.
        StopAllCoroutines();

        if (chargeEffect != null)
            chargeEffect.SetActive(false);

        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (meleePoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            meleePoint.position,
            meleeHitRadius
        );
    }



    private void TryTeleport()
    {
        if (player == null ||
            agent == null ||
            !agent.isOnNavMesh ||
            teleportPoints == null ||
            teleportPoints.Points == null)
        {
            return;
        }

        Transform[] points = teleportPoints.Points;

        // Store every valid teleport destination.
        List<Vector3> validPositions = new List<Vector3>();
        List<int> validIndices = new List<int>();

        // Check ALL teleport points.
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];

            if (point == null)
                continue;

            // Find a valid NavMesh position near this point.
            NavMeshHit hit;

            if (!NavMesh.SamplePosition(
                point.position,
                out hit,
                0.75f,
                agent.areaMask))
            {
                continue;
            }

            Vector3 destination = hit.position;

            // Check distance from the Imp.
            Vector3 offset = destination - transform.position;
            offset.y = 0f;

            float distance = offset.magnitude;

            if (distance < minTeleportDistance ||
                distance > maxTeleportDistance)
            {
                continue;
            }

            // Don't appear right next to the player.
            Vector3 playerOffset = destination - player.position;
            playerOffset.y = 0f;

            if (playerOffset.magnitude < minDistanceFromPlayer)
            {
                continue;
            }

            // Make sure there's a complete NavMesh path.
            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(destination, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            // This location is valid!
            validPositions.Add(destination);
            validIndices.Add(i);
        }

        // Helpful debugging information.
        Debug.Log(
            "Imp found " + validPositions.Count +
            " valid teleport points out of " + points.Length
        );

        if (validPositions.Count == 0)
            return;

        // Try to avoid repeating the previous location.
        List<int> choices = new List<int>();

        for (int i = 0; i < validIndices.Count; i++)
        {
            if (validIndices[i] != lastTeleportIndex)
            {
                choices.Add(i);
            }
        }

        // If there are no alternatives, allow the old point.
        if (choices.Count == 0)
        {
            for (int i = 0; i < validIndices.Count; i++)
            {
                choices.Add(i);
            }
        }

        // Randomly pick from ALL eligible locations.
        int randomChoice = Random.Range(0, choices.Count);
        int chosen = choices[randomChoice];

        Vector3 destinationPosition = validPositions[chosen];

        Vector3 oldPosition = transform.position;

        // Stop navigation before teleporting.
        agent.ResetPath();

        // Teleport using the NavMeshAgent.
        if (!agent.Warp(destinationPosition))
            return;

        lastTeleportIndex = validIndices[chosen];

        // Play teleport effects.
        if (teleportEffectPrefab != null)
        {
            GameObject departure = Instantiate(
                teleportEffectPrefab,
                oldPosition,
                Quaternion.identity
            );

            GameObject arrival = Instantiate(
                teleportEffectPrefab,
                destinationPosition,
                Quaternion.identity
            );

            Destroy(departure, 2f);
            Destroy(arrival, 2f);
        }

        nextTeleportTime = Time.time + teleportCooldown;

        Debug.Log(
            "Imp teleported to " +
            points[lastTeleportIndex].name
        );
    }


}
