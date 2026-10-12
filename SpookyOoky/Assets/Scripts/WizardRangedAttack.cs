

using UnityEngine;
using System.Collections;

public class WizardRangedAttack : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 7f;

    [Header("Attack Settings")]
    public float attackRange = 14f;
    public float attackCooldown = 2.5f;
    public float rotationSpeed = 5f;

    [Header("Casting")]
    public float castingTime = 0.8f;
    public GameObject chargeEffect;

    private Transform player;
    private float attackTimer;
    private bool isCasting = false;

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        attackTimer = attackCooldown;

        if (chargeEffect != null)
            chargeEffect.SetActive(false);
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        // Don't start attacking from too far away.
        if (distance > attackRange && !isCasting)
            return;

        // Face the player.
        Vector3 direction = player.position - transform.position;
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

        // Don't start another spell while casting.
        if (isCasting)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            StartCoroutine(CastSpell());
        }
    }

    private IEnumerator CastSpell()
    {
        isCasting = true;

        // Show the magical charging effect.
        if (chargeEffect != null)
            chargeEffect.SetActive(true);

        // Give the player time to react.
        yield return new WaitForSeconds(castingTime);

        // Hide the charging effect.
        if (chargeEffect != null)
            chargeEffect.SetActive(false);

        // Fire the spell if the player still exists
        // and is within attack range.
        if (player != null &&
            Vector3.Distance(transform.position, player.position)
                <= attackRange)
        {
            Shoot();
        }

        attackTimer = attackCooldown;
        isCasting = false;
    }

    private void Shoot()
    {
        if (projectilePrefab == null ||
            firePoint == null ||
            player == null)
            return;

        Vector3 targetPosition =
            player.position + Vector3.up;

        Vector3 shootDirection =
            (targetPosition - firePoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        WizardProjectile projectileScript =
            projectile.GetComponent<WizardProjectile>();

        if (projectileScript != null)
            projectileScript.SetOwner(transform);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                shootDirection * projectileSpeed;
        }
    }

    private void OnDisable()
    {
        // Stop any unfinished casting when the Wizard dies.
        StopAllCoroutines();
        isCasting = false;

        if (chargeEffect != null)
            chargeEffect.SetActive(false);
    }
}
