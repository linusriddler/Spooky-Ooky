using UnityEditor;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject Enemy;
    private bool enemySpawned;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemySpawned == false)
        {
            Instantiate(Enemy, transform.position, transform.rotation);
        }
    }
}
