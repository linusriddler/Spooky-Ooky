using UnityEngine;
using AZE.AdvancedFirstPerson;

public class FireballAbility : MonoBehaviour
{
    public GameObject fireballPrefab;
    public Transform firePoint;
    public bool FireballTriggered = true;
    public float fireballSpeed = 20f;
    private PlayerInputHandler inputHandler;
    public Transform playerCamera;
    public float fireballCooldown = 2f;
    private float nextFireballTime = 0f;
    public float aimDistance = 100f;

    void Start()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        if (inputHandler.FireballTriggered && Time.time >= nextFireballTime)
        {
            ShootFireball();
            nextFireballTime = Time.time + fireballCooldown;
        }
    }

    void ShootFireball()
    {
        // Find the point the center of the camera is aiming at
        Vector3 aimPoint = playerCamera.position
                         + playerCamera.forward * aimDistance;
        if (Physics.Raycast(
       playerCamera.position,
       playerCamera.forward,
       out RaycastHit hit,
       aimDistance))
        {
            aimPoint = hit.point;
        }

        // Aim from the FirePoint toward that point
        Vector3 shootDirection =
            (aimPoint - firePoint.position).normalized;

        // Spawn the fireball facing its shooting direction
        GameObject fireball = Instantiate(
            fireballPrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );


        // Get the Rigidbody on the fireball
        Rigidbody rb = fireball.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = shootDirection * fireballSpeed;
        }

        // Keep the fireball from hitting the player who fired it
        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        Collider[] fireballColliders =
            fireball.GetComponentsInChildren<Collider>();

        foreach (Collider playerCollider in playerColliders)
        {
            foreach (Collider fireballCollider in fireballColliders)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    fireballCollider
                );
            }
        }
    }
}