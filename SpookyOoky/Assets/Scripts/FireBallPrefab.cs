using UnityEngine;

public class Fireball : MonoBehaviour
{
    // How long the fireball can exist if it doesn't hit anything
    public float lifetime = 5f;
    public GameObject impactEffect;
    public float impactEffectLifetime = 3f;

    private bool hasHit = false;

    void Start()
    {
        // Destroy the fireball after 5 seconds
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple collisions
        if (hasHit) 
            return; 

        hasHit = true;

        // Find the exact point where the fireball hit
        ContactPoint contact = collision.GetContact(0);

        // Spawn the explosion at the impact point
        if (impactEffect != null)
        {
            GameObject explosion = Instantiate(
                impactEffect,
                contact.point,
                Quaternion.identity
            );

            // Clean up the explosion after it has played
            Destroy(explosion, impactEffectLifetime);
        }


        // Destroy the fireball when it hits something
        Destroy(gameObject);
    }
}