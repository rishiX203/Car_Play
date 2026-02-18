using UnityEngine;

public class RoadTile : MonoBehaviour
{
    [Header("Tile Configuration")]
    [Tooltip("Exact length of this road tile on X-axis")]
    public float tileLength = 50f;

    [Header("Building Spawn Points")]
    [Tooltip("Drag spawn point GameObjects here")]
    public Transform[] leftSpawnPoints;
    public Transform[] rightSpawnPoints;

    [Header("Debug")]
    public bool showGizmos = true;

    private bool hasSpawnedNext = false;

    void OnTriggerEnter(Collider other)
    {
        // Check if player car entered the spawn trigger
        if (other.CompareTag("Player") && !hasSpawnedNext)
        {
            hasSpawnedNext = true;

            // Find RoadManager and spawn next tile (Unity 6 compatible)
            RoadManager roadManager = FindFirstObjectByType<RoadManager>();
            if (roadManager != null)
            {
                roadManager.SpawnNextRoadTile();
            }
            else
            {
                Debug.LogError("RoadManager not found in scene!");
            }
        }
    }

    public void ResetTile()
    {
        hasSpawnedNext = false;
    }

    public Transform GetRandomLeftSpawnPoint()
    {
        if (leftSpawnPoints == null || leftSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No left spawn points assigned on " + gameObject.name);
            return null;
        }
        return leftSpawnPoints[Random.Range(0, leftSpawnPoints.Length)];
    }

    public Transform GetRandomRightSpawnPoint()
    {
        if (rightSpawnPoints == null || rightSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No right spawn points assigned on " + gameObject.name);
            return null;
        }
        return rightSpawnPoints[Random.Range(0, rightSpawnPoints.Length)];
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw tile bounds (green box)
        Gizmos.color = Color.green;
        Vector3 center = transform.position + new Vector3(tileLength * 0.5f, 0, 0);
        Vector3 size = new Vector3(tileLength, 0.5f, 10);
        Gizmos.DrawWireCube(center, size);

        // Draw left spawn points (blue spheres)
        if (leftSpawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in leftSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }

        // Draw right spawn points (red spheres)
        if (rightSpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in rightSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}