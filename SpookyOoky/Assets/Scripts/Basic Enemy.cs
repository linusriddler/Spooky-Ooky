using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    public float stopDistance = 2f;
    private bool isDead;
    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0f;
            transform.rotation = Quaternion.LookRotation(lookPos);
        }
    }
}

