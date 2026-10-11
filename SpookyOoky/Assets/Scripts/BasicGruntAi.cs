using UnityEngine;

public class BasicGruntAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.4f;
    public float turnSpeed = 8f;

    private Transform player;

    private void Start()
    {
        // Find the player when the enemy spawns.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "Basic Grunt could not find the Player! " +
                "Make sure your player has the Player tag."
            );
        }
    }

    private void Update()
    {
        // Do nothing if we couldn't find the player.
        if (player == null)
            return;

        // Find the direction from the enemy to the player.
        Vector3 direction =
            player.position - transform.position;

        // Keep movement horizontal so the enemy's
        // idle floating animation can continue normally.
        direction.y = 0f;

        float distance = direction.magnitude;

        // Stop moving when close enough to attack.
        if (distance <= stopDistance)
            return;

        direction.Normalize();

        // Move toward the player.
        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        // Gradually turn toward the player.
        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }
}