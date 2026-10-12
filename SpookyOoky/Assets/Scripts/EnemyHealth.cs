using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 30f;

    [Header("Death Effect")]
    public GameObject deathEffectPrefab;
    public float deathEffectLifetime = 1f;

    [Header("Death Timing")]
    public float deathEffectDelay = 0f;
    public float bodyDisappearDelay = 1f;

    private bool isDead;

    private float currentHealth;

    private void Awake()
    {
        // Start with full health.
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // Don't process damage if the enemy is already dead.
        if (currentHealth <= 0f)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // Stop the enemy's movement and attacks immediately.
        BasicGruntAI ai = GetComponent<BasicGruntAI>();
        if (ai != null)
            ai.enabled = false;

        BasicGruntMeleeAttack melee =
            GetComponent<BasicGruntMeleeAttack>();

        if (melee != null)
            melee.enabled = false;
        
        // Stop Wizard attacks after death.
        WizardRangedAttack wizard =
            GetComponent<WizardRangedAttack>();

        if (wizard != null)
        {
            wizard.enabled = false;
        }

        // Stop the Wizard's retreat behavior when defeated.
        WizardRetreatAI retreatAI =
            GetComponent<WizardRetreatAI>();

        if (retreatAI != null)
        {
            retreatAI.enabled = false;
        }

        // Stop the Imp's combat behavior when defeated.
        ImpCombatAI impAI = GetComponent<ImpCombatAI>();

        if (impAI != null)
        {
            impAI.enabled = false;
        }

        // Prevent the dead enemy from blocking the player.
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Keep the skeleton visible while the death sequence plays.
        StartCoroutine(DeathSequence());

        
    }

    private IEnumerator DeathSequence()
    {
        // Give the fireball explosion time to clear.
        yield return new WaitForSeconds(deathEffectDelay);

        // Spawn the death particles independently.
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                deathEffectPrefab,
                GetVisualCenter(),
                Quaternion.identity
            );

            Destroy(effect, deathEffectLifetime);
        }

        // Briefly let the particles appear around the skeleton.
        yield return new WaitForSeconds(bodyDisappearDelay);

        // Now remove the skeleton visuals.
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;

        Destroy(gameObject);
    }

    private Vector3 GetVisualCenter()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        // Fallback in case the enemy has no renderers.
        if (renderers.Length == 0)
            return transform.position + Vector3.up;

        // Start with the first visible model's bounds.
        Bounds combinedBounds = renderers[0].bounds;

        // Expand the bounds to include the entire model.
        foreach (Renderer rend in renderers)
        {
            combinedBounds.Encapsulate(rend.bounds);
        }

        // Return the center of the combined model bounds.
        return combinedBounds.center;
    }

}