using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(Player, transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
