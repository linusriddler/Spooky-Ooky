
using UnityEngine;

public class WizardProjectile : MonoBehaviour
{
    public float damage = 12f;
    public float lifetime = 6f;

    private Transform owner;

    private void Start()
    {
        // Prevent projectiles from lasting forever.
        Destroy(gameObject, lifetime);
    }

    // Called when the Wizard fires this projectile.
    public void SetOwner(Transform wizard)
    {
        owner = wizard;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore the Wizard that fired the projectile.
        if (owner != null && other.transform.IsChildOf(owner))
            return;

        // Check whether we hit the player.
        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // Destroy the projectile on impact.
        Destroy(gameObject);
    }
}

