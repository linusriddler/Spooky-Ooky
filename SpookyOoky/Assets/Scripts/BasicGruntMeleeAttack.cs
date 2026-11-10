
using UnityEngine;
using System.Collections;

public class BasicGruntMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.7f;
    public float attackCooldown = 2f;
    public float attackWindup = 0.25f;
    public float damage = 8f;

    [Header("Slash Effect")]
    public GameObject slashPrefab;
    public Transform attackPoint;
    public float slashLifetime = 1f;

    [Header("Hitbox Settings")]
    public Vector3 hitboxHalfExtents =
        new Vector3(0.65f, 0.65f, 0.65f);

    private Transform player;
    private bool isAttacking;
    private float nextAttackTime;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "Grunt cannot find the Player. Check the Player tag."
            );
        }

        if (attackPoint == null)
        {
            Debug.LogWarning(
                "Assign an AttackPoint on " + gameObject.name
            );
        }
    }

    private void Update()
    {
        if (player == null || attackPoint == null)
            return;

        // Measure distance horizontally.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        // Only attack when the player is close enough.
        if (distance > attackRange)
            return;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                12f * Time.deltaTime
            );
        }

        // Wait until the attack cooldown has finished.
        if (!isAttacking && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        // Face the player before starting the attack.
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Spawn the animated slash visual.
        if (slashPrefab != null)
        {
            GameObject slash = Instantiate(
                slashPrefab,
                attackPoint.position,
                transform.rotation
            );

            Destroy(slash, slashLifetime);
        }

        // Give the player a brief moment to dodge.
        yield return new WaitForSeconds(attackWindup);

        // Check the hitbox at the moment of impact.
        if (player != null)
        {
            Vector3 currentDirection =
                player.position - transform.position;

            currentDirection.y = 0f;

            Quaternion hitRotation =
                currentDirection.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(currentDirection)
                : transform.rotation;

            Collider[] hits = Physics.OverlapBox(
                attackPoint.position,
                hitboxHalfExtents,
                hitRotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider hit in hits)
            {
                // Only damage the player, not the environment
                // or other enemies.
                bool isPlayer =
                    hit.transform == player ||
                    hit.transform.IsChildOf(player);

                if (!isPlayer)
                    continue;

                // Calls TakeDamage(float) if the player has it.
                hit.SendMessageUpwards(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );

                Debug.Log(
                    "Grunt hit the player for " + damage + " damage."
                );

                // Prevent multiple hits from multiple colliders.
                break;
            }
        }

        isAttacking = false;
    }

    // Show the attack hitbox when the enemy is selected.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            attackPoint.position,
            attackPoint.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            hitboxHalfExtents * 2f
        );
    }
}
