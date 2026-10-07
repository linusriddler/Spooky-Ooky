using UnityEngine;

public class FireballAbility : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;

    public float fireballSpeed = 20f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShootFireball();
        }
    }

    void ShootFireball()
    {
        // Create the fireball at the FirePoint
        GameObject fireball = Instantiate(
            fireballPrefab,
            firePoint.position,
            firePoint.rotation
        );

        // Get the Rigidbody on the fireball
        Rigidbody rb = fireball.GetComponent<Rigidbody>();

        // Make the fireball fly forward
        rb.linearVelocity = firePoint.forward * fireballSpeed;
    }
}