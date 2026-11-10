
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BasicGruntAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 1.4f;

    private Transform player;
    private NavMeshAgent agent;

    private void Start()
    {
        // Find the player.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Get the navigation component.
        agent = GetComponent<NavMeshAgent>();

        // Set movement speed and stopping distance.
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        // Make sure the player exists.
        if (player == null)
            return;

        // Only navigate if the enemy is on a NavMesh.
        if (!agent.isOnNavMesh)
            return;

        // Tell Unity to find a path toward the player.
        agent.SetDestination(player.position);
    }

    private void OnDisable()
    {
        // Stop moving when this AI is disabled,
        // including when the Grunt dies.
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }
}
