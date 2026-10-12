using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class AnimatedSlashAbility : MonoBehaviour
{
    [Header("References")]
    public Animator meleeAnimator;
    public Transform attackOrigin;

    [Header("Attack Settings")]
    public float damage = 15f;
    public float cooldown = 0.6f;
    public float range = 3f;
    public float hitRadius = 2f;
    public float coneAngle = 70f;
    public float knockbackDistance = 1.25f;

    [Header("Timing")]
    public float attack1HitTime = 0.13f;
    public float attack2HitTime = 0.16f;
    public float recoveryTime = 0.25f;

    private bool isAttacking = false;
    private bool useAttack1 = true;
    private float nextAttackTime = 0f;

    private static readonly int Attack1Hash =
        Animator.StringToHash("Attack 1");

    private static readonly int Attack2Hash =
        Animator.StringToHash("Attack 2");

    [Header("Spectral Trail")]
    public TrailRenderer spectralTrail;
    public float trailStartDelay = 0.07f;
    public float trailDuration = 0.18f;


    private void Start()
    {
        if (spectralTrail != null)
        {
            spectralTrail.emitting = false;
            spectralTrail.Clear();
        }
    }
    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.fKey.wasPressedThisFrame)
            return;

        if (Time.timeScale == 0f || isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (meleeAnimator == null || attackOrigin == null)
            return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + cooldown;

        bool doingAttack1 = useAttack1;
        useAttack1 = !useAttack1;

        int attackHash = doingAttack1 ? Attack1Hash : Attack2Hash;
        float hitTime = doingAttack1 ? attack1HitTime : attack2HitTime;

        meleeAnimator.CrossFadeInFixedTime(attackHash, 0.05f);

        StartCoroutine(PlaySlashTrail());

        yield return new WaitForSeconds(hitTime);

        PerformHit();

        yield return new WaitForSeconds(recoveryTime);

        isAttacking = false;
    }

    private void PerformHit()
    {
        Vector3 hitCenter = attackOrigin.position + attackOrigin.forward * 1.5f;

        Collider[] hits = Physics.OverlapSphere(
            hitCenter,
            hitRadius,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null || hitEnemies.Contains(enemy))
                continue;

            Vector3 toEnemy = hit.bounds.center - attackOrigin.position;
            float distance = toEnemy.magnitude;

            if (distance > range || distance < 0.001f)
                continue;

            float angle = Vector3.Angle(attackOrigin.forward, toEnemy);

            if (angle > coneAngle * 0.5f)
                continue;

            // Optional line-of-sight check
            if (Physics.Raycast(
                attackOrigin.position,
                toEnemy.normalized,
                out RaycastHit obstruction,
                distance + 0.05f,
                ~0,
                QueryTriggerInteraction.Ignore))
            {
                EnemyHealth hitEnemy =
                    obstruction.collider.GetComponentInParent<EnemyHealth>();

                if (hitEnemy != enemy)
                    continue;
            }

            hitEnemies.Add(enemy);

            
            enemy.TakeDamage(damage);
        }
    }

    private void ApplyKnockback(EnemyHealth enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        Vector3 direction = enemy.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        Vector3 start = agent.nextPosition;
        Vector3 target = start + direction * knockbackDistance;

        if (NavMesh.Raycast(
            start,
            target,
            out NavMeshHit wallHit,
            agent.areaMask))
        {
            target = Vector3.MoveTowards(
                start,
                wallHit.position,
                Mathf.Max(0f, Vector3.Distance(start, wallHit.position) - 0.2f)
            );
        }

        if (NavMesh.SamplePosition(
            target,
            out NavMeshHit navHit,
            0.35f,
            agent.areaMask))
        {
            if (!NavMesh.Raycast(
                start,
                navHit.position,
                out NavMeshHit blockingHit,
                agent.areaMask))
            {
                agent.Warp(navHit.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null)
            return;

        Gizmos.color = Color.cyan;
        Vector3 hitCenter = attackOrigin.position + attackOrigin.forward * 1.5f;
        Gizmos.DrawWireSphere(hitCenter, hitRadius);
    }

    private IEnumerator PlaySlashTrail()
    {
        if (spectralTrail == null)
            yield break;

        // Wait for the actual sword swing to begin.
        yield return new WaitForSeconds(trailStartDelay);

        // Start drawing the magical trail.
        spectralTrail.Clear();
        spectralTrail.emitting = true;

        // Keep the trail active during the cut.
        yield return new WaitForSeconds(trailDuration);

        // Stop creating new trail segments.
        spectralTrail.emitting = false;
    }
}