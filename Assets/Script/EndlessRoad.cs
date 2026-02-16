using System.Collections.Generic;
using UnityEngine;

public class EndlessRoad : MonoBehaviour
{
    public GameObject roadPrefab;
    public Transform player;

    public int initialRoadCount = 5;
    public float roadLength = 50f;

    private float spawnZ = 0;
    private List<GameObject> activeRoads = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoad();
        }
    }

    void Update()
    {
        if (player.position.z > spawnZ - (initialRoadCount * roadLength))
        {
            SpawnRoad();
            DeleteRoad();
        }
    }

    void SpawnRoad()
    {
        GameObject road = Instantiate(roadPrefab, Vector3.forward * spawnZ, Quaternion.identity);
        activeRoads.Add(road);
        spawnZ += roadLength;
    }

    void DeleteRoad()
    {
        Destroy(activeRoads[0]);
        activeRoads.RemoveAt(0);
    }
}
