
using UnityEngine;

public class ImpTeleportPoints : MonoBehaviour
{
    // All teleport locations under this object.
    public Transform[] Points { get; private set; }

    private void Awake()
    {
        // Automatically collect every direct child.
        Points = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            Points[i] = transform.GetChild(i);
        }
    }
}
