using UnityEngine;
using System.Collections.Generic;

public class RoadManager : MonoBehaviour
{
    [Header("Road Configuration")]
    [Tooltip("Drag your road tile prefabs here (can add multiple for variety)")]
    public GameObject[] roadTilePrefabs;

    [Tooltip("How many tiles to spawn at game start")]
    public int initialTileCount = 5;

    [Tooltip("Must match the X-axis length of your road tiles")]
    public float tileLength = 50f;

    [Header("Building Configuration")]
    [Tooltip("Drag all building prefabs here")]
    public GameObject[] buildingPrefabs;

    [Tooltip("Minimum buildings per side of road")]
    [Range(0, 5)]
    public int minBuildingsPerSide = 1;

    [Tooltip("Maximum buildings per side of road")]
    [Range(1, 5)]
    public int maxBuildingsPerSide = 3;

    [Tooltip("Chance to skip spawning building at a point (0-1)")]
    [Range(0f, 1f)]
    public float buildingSkipChance = 0.3f;

    [Header("Performance Settings")]
    [Tooltip("Maximum tiles to keep in pool")]
    public int maxPoolSize = 10;

    [Tooltip("How far behind player before recycling tile")]
    public float recycleDistance = 60f;

    [Header("Debug")]
    public bool showDebugLogs = true;
    public bool showOnScreenDebug = true;

    // Private variables
    private float nextSpawnX = 0f;
    private Transform playerTransform;

    // Object pools
    private List<GameObject> activeRoadTiles = new List<GameObject>();
    private Queue<GameObject> roadTilePool = new Queue<GameObject>();
    private Queue<GameObject> buildingPool = new Queue<GameObject>();

    // Track buildings on tiles
    private Dictionary<GameObject, List<GameObject>> tileBuildingsMap = new Dictionary<GameObject, List<GameObject>>();

    void Start()
    {
        Debug.Log("=== ROAD MANAGER STARTING ===");

        // Find player car
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("CRITICAL: No GameObject with 'Player' tag found! Tag your car as 'Player'!");
            enabled = false;
            return;
        }
        playerTransform = player.transform;
        Debug.Log("Player found at: " + playerTransform.position);

        // Validate road tile prefabs
        if (roadTilePrefabs == null || roadTilePrefabs.Length == 0)
        {
            Debug.LogError("CRITICAL: No road tile prefabs assigned to RoadManager!");
            enabled = false;
            return;
        }
        Debug.Log("Road tile prefabs assigned: " + roadTilePrefabs.Length);

        // Validate building prefabs
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogWarning("WARNING: No building prefabs assigned. Roads will spawn without buildings.");
        }
        else
        {
            Debug.Log("Building prefabs assigned: " + buildingPrefabs.Length);
        }

        // Initialize object pools
        InitializePools();

        // Spawn initial road tiles
        for (int i = 0; i < initialTileCount; i++)
        {
            SpawnNextRoadTile();
        }

        Debug.Log("=== ROAD MANAGER READY ===");
        Debug.Log("Initial tiles spawned: " + initialTileCount);
        Debug.Log("Active tiles: " + activeRoadTiles.Count);
    }

    void InitializePools()
    {
        Debug.Log("Initializing object pools...");

        // Pre-create some road tiles for pool
        for (int i = 0; i < 3; i++)
        {
            GameObject tile = Instantiate(GetRandomRoadPrefab());
            tile.SetActive(false);
            tile.transform.SetParent(transform);
            roadTilePool.Enqueue(tile);
        }

        // Pre-create buildings for pool
        if (buildingPrefabs != null && buildingPrefabs.Length > 0)
        {
            foreach (GameObject buildingPrefab in buildingPrefabs)
            {
                for (int i = 0; i < 2; i++)
                {
                    GameObject building = Instantiate(buildingPrefab);
                    building.SetActive(false);
                    building.transform.SetParent(transform);
                    buildingPool.Enqueue(building);
                }
            }
        }

        Debug.Log("Pools initialized - Tiles: " + roadTilePool.Count + ", Buildings: " + buildingPool.Count);
    }

    public void SpawnNextRoadTile()
    {
        // Get tile from pool or create new
        GameObject newTile = GetTileFromPool();

        // Position the tile
        newTile.transform.position = new Vector3(nextSpawnX, 0, 0);
        newTile.SetActive(true);

        // Reset tile state
        RoadTile roadTileScript = newTile.GetComponent<RoadTile>();
        if (roadTileScript != null)
        {
            roadTileScript.ResetTile();
        }
        else
        {
            Debug.LogWarning("Road tile prefab is missing RoadTile script component!");
        }

        // Add to active tiles list
        activeRoadTiles.Add(newTile);

        // Spawn buildings on this tile
        List<GameObject> spawnedBuildings = SpawnBuildingsOnTile(newTile);
        tileBuildingsMap[newTile] = spawnedBuildings;

        if (showDebugLogs)
        {
            Debug.Log("Spawned tile at X=" + nextSpawnX + " with " + spawnedBuildings.Count + " buildings");
        }

        // Update next spawn position
        nextSpawnX += tileLength;
    }

    GameObject GetTileFromPool()
    {
        // Try to get from pool first
        if (roadTilePool.Count > 0)
        {
            return roadTilePool.Dequeue();
        }

        // Create new if pool is empty
        GameObject newTile = Instantiate(GetRandomRoadPrefab());
        newTile.transform.SetParent(transform);

        if (showDebugLogs)
        {
            Debug.Log("Created new tile (pool was empty)");
        }

        return newTile;
    }

    GameObject GetRandomRoadPrefab()
    {
        if (roadTilePrefabs == null || roadTilePrefabs.Length == 0)
        {
            Debug.LogError("No road tile prefabs available!");
            return null;
        }
        return roadTilePrefabs[Random.Range(0, roadTilePrefabs.Length)];
    }

    List<GameObject> SpawnBuildingsOnTile(GameObject tile)
    {
        List<GameObject> spawnedBuildings = new List<GameObject>();

        // Check if we have building prefabs
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            return spawnedBuildings;
        }

        // Get the RoadTile script
        RoadTile roadTile = tile.GetComponent<RoadTile>();
        if (roadTile == null)
        {
            Debug.LogWarning("Tile has no RoadTile component - cannot spawn buildings");
            return spawnedBuildings;
        }

        // Spawn buildings on left side
        int leftCount = Random.Range(minBuildingsPerSide, maxBuildingsPerSide + 1);
        for (int i = 0; i < leftCount; i++)
        {
            // Random chance to skip this spawn point
            if (Random.value < buildingSkipChance)
            {
                continue;
            }

            Transform spawnPoint = roadTile.GetRandomLeftSpawnPoint();
            if (spawnPoint != null)
            {
                GameObject building = SpawnBuilding(spawnPoint.position, spawnPoint.rotation);
                if (building != null)
                {
                    spawnedBuildings.Add(building);
                }
            }
        }

        // Spawn buildings on right side
        int rightCount = Random.Range(minBuildingsPerSide, maxBuildingsPerSide + 1);
        for (int i = 0; i < rightCount; i++)
        {
            // Random chance to skip this spawn point
            if (Random.value < buildingSkipChance)
            {
                continue;
            }

            Transform spawnPoint = roadTile.GetRandomRightSpawnPoint();
            if (spawnPoint != null)
            {
                GameObject building = SpawnBuilding(spawnPoint.position, spawnPoint.rotation);
                if (building != null)
                {
                    spawnedBuildings.Add(building);
                }
            }
        }

        return spawnedBuildings;
    }

    GameObject SpawnBuilding(Vector3 position, Quaternion rotation)
    {
        GameObject building = GetBuildingFromPool();
        if (building == null)
        {
            return null;
        }

        // Set position and rotation
        building.transform.position = position;
        building.transform.rotation = rotation;

        // Add random rotation variation
        building.transform.Rotate(0, Random.Range(-10f, 10f), 0);

        // Activate the building
        building.SetActive(true);

        return building;
    }

    GameObject GetBuildingFromPool()
    {
        // Try to get from pool first
        if (buildingPool.Count > 0)
        {
            return buildingPool.Dequeue();
        }

        // Create new if pool is empty
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            return null;
        }

        GameObject randomBuilding = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
        GameObject newBuilding = Instantiate(randomBuilding);
        newBuilding.transform.SetParent(transform);

        return newBuilding;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        // Recycle old tiles that are behind the player
        RecycleOldTiles();
    }

    void RecycleOldTiles()
    {
        // Loop backwards through active tiles
        for (int i = activeRoadTiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = activeRoadTiles[i];

            if (tile == null)
            {
                activeRoadTiles.RemoveAt(i);
                continue;
            }

            // Calculate distance behind player
            float distanceBehind = playerTransform.position.x - tile.transform.position.x;

            // Check if tile should be recycled
            if (distanceBehind > recycleDistance)
            {
                // Recycle all buildings on this tile
                if (tileBuildingsMap.ContainsKey(tile))
                {
                    foreach (GameObject building in tileBuildingsMap[tile])
                    {
                        if (building != null)
                        {
                            building.SetActive(false);
                            buildingPool.Enqueue(building);
                        }
                    }
                    tileBuildingsMap.Remove(tile);
                }

                // Recycle the tile itself
                tile.SetActive(false);
                activeRoadTiles.RemoveAt(i);

                // Return to pool if not exceeding max size
                if (roadTilePool.Count < maxPoolSize)
                {
                    roadTilePool.Enqueue(tile);
                }
                else
                {
                    Destroy(tile);
                }

                if (showDebugLogs)
                {
                    Debug.Log("Recycled tile at distance: " + distanceBehind.ToString("F1"));
                }
            }
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmos()
    {
        if (playerTransform == null)
        {
            return;
        }

        // Draw recycle line (red) - Using UnityEngine.Color explicitly
        Gizmos.color = UnityEngine.Color.red;
        Vector3 recyclePos = playerTransform.position - new Vector3(recycleDistance, 0, 0);
        Gizmos.DrawLine(recyclePos + Vector3.up * 10, recyclePos - Vector3.up * 10);

        // Draw spawn line (green)
        Gizmos.color = UnityEngine.Color.green;
        Vector3 spawnPos = new Vector3(nextSpawnX, 0, 0);
        Gizmos.DrawLine(spawnPos + Vector3.up * 10, spawnPos - Vector3.up * 10);
    }

    // On-screen debug info
    void OnGUI()
    {
        if (!showOnScreenDebug)
        {
            return;
        }

        GUIStyle style = new GUIStyle();
        style.fontSize = 18;
        style.normal.textColor = UnityEngine.Color.white;  // Explicitly use UnityEngine.Color
        style.fontStyle = FontStyle.Bold;

        // Background box
        GUI.Box(new Rect(10, 10, 300, 150), "");

        // Debug info
        GUI.Label(new Rect(20, 15, 280, 25), "=== ROAD MANAGER DEBUG ===", style);
        GUI.Label(new Rect(20, 45, 280, 25), "Active Tiles: " + activeRoadTiles.Count, style);
        GUI.Label(new Rect(20, 70, 280, 25), "Pooled Tiles: " + roadTilePool.Count, style);
        GUI.Label(new Rect(20, 95, 280, 25), "Pooled Buildings: " + buildingPool.Count, style);

        if (playerTransform != null)
        {
            GUI.Label(new Rect(20, 120, 280, 25), "Car Position X: " + playerTransform.position.x.ToString("F1"), style);
        }
    }

    // Cleanup
    void OnDestroy()
    {
        roadTilePool.Clear();
        buildingPool.Clear();
        activeRoadTiles.Clear();
        tileBuildingsMap.Clear();
    }

    // Public helper methods
    public int GetActiveRoadTileCount()
    {
        return activeRoadTiles.Count;
    }

    public float GetNextSpawnPosition()
    {
        return nextSpawnX;
    }
}