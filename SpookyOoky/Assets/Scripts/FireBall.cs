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

    void Start()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        if (inputHandler.FireballTriggered)
        {
            ShootFireball();
        }
    }

    void ShootFireball()
    {
        // Create the fireball at the FirePoint
        GameObject fireball = Instantiate
        (
            fireballPrefab,
            firePoint.position,
            playerCamera.rotation
        );

        // Get the Rigidbody on the fireball
        Rigidbody rb = fireball.GetComponent<Rigidbody>();



        // Make the fireball fly forward
        rb.linearVelocity = playerCamera.forward * fireballSpeed;

        // Find all colliders on the player
        Collider[] playerColliders = GetComponentsInChildren<Collider>();

        // Find all colliders on the fireball
        Collider[] fireballColliders = fireball.GetComponentsInChildren<Collider>();

        // Make the fireball ignore the player's colliders
        foreach (Collider playerCollider in playerColliders)
        {
            foreach (Collider fireballCollider in fireballColliders)
            {
                Physics.IgnoreCollision(playerCollider, fireballCollider);
            }
        }
    }
}