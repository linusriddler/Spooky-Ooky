using UnityEditor;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject Enemy;
    private GameObject currentEnemy;
    void Update()
    {
        if (currentEnemy == null)
        {
            currentEnemy = Instantiate(Enemy, transform.position, transform.rotation);
        }
    }
}