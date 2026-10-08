using UnityEngine;

public class Fireball : MonoBehaviour
{
    // How long the fireball can exist if it doesn't hit anything
    public float lifetime = 5f;

    void Start()
    {
        // Destroy the fireball after 5 seconds
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy the fireball when it hits something
        Destroy(gameObject);
    }
}