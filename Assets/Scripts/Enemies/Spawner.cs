using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject dronePrefab;

    [Header("Spawn Settings")]
    public float enemySpawnInterval = 3f;
    private float enemyTimer;

    [Header("Spawn Points")]
    public Transform[] enemySpawnPoints;

    // Reference to teleporter so we can use its arrivalPoints
    [Header("Teleporter Link")]
    public TVScreenTeleporter teleporter;

    public ObjectPool enemyPool;
    public ObjectPool dronePool;
    public DayNightCycle cycle;

    void Awake()
    {
        enemyPool = CreatePool(enemyPrefab, "EnemyPool");
        dronePool = CreatePool(dronePrefab, "DronePool");
    }

    void Start()
    {
        // Disable all drone points initially
        DisableAllDronePoints();

        // Sync with teleporter’s current camera index if available
        if (teleporter != null && teleporter.tv != null)
            SetActiveDroneSpawnIndex(teleporter.tv.ActiveIndex);
    }

    void Update()
    {
        if (cycle != null && cycle.isNight)
            HandleEnemySpawning();

        if (Input.GetKeyDown(KeyCode.F))
            SpawnDrone();
    }

    // ---------------- Enemy spawning ----------------
    void HandleEnemySpawning()
    {
        enemyTimer += Time.deltaTime;

        if (enemyTimer >= enemySpawnInterval)
        {
            enemyTimer = 0f;
            SpawnEnemy();
        }
    }

    public GameObject SpawnEnemy()
    {
        if (enemySpawnPoints.Length == 0) return null;

        Transform point = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
        GameObject enemy = enemyPool.Get(point.position, point.rotation);
        enemy.tag = "Enemy";

        return enemy;
    }

    // ---------------- Drone spawning ----------------
    public void DisableAllDronePoints()
    {
        if (teleporter == null || teleporter.arrivalPoints == null) return;

        foreach (var p in teleporter.arrivalPoints)
            if (p != null)
                p.gameObject.SetActive(false);

        Debug.Log("[Spawner] All drone spawn points DISABLED");
    }

    public void SetActiveDroneSpawnIndex(int index)
    {
        Debug.Log($"[Spawner] Received index {index}");

        DisableAllDronePoints();

        if (teleporter == null || teleporter.arrivalPoints == null) return;

        if (index < 0 || index >= teleporter.arrivalPoints.Length)
        {
            Debug.LogWarning("[Spawner] Invalid index! No spawn point exists at that index.");
            return;
        }

        if (teleporter.arrivalPoints[index] == null)
        {
            Debug.LogWarning("[Spawner] Arrival point is NULL at index " + index);
            return;
        }

        teleporter.arrivalPoints[index].gameObject.SetActive(true);

        Debug.Log($"[Spawner] ENABLED drone spawn point[{index}] at {teleporter.arrivalPoints[index].position}");
    }

    public GameObject SpawnDrone()
    {
        if (teleporter == null || teleporter.tv == null) return null;

        int activeIndex = teleporter.tv.ActiveIndex;
        if (activeIndex < 0 || activeIndex >= teleporter.arrivalPoints.Length)
        {
            Debug.LogWarning("[Spawner] Cannot spawn drone — Invalid active index.");
            return null;
        }

        Transform activePoint = teleporter.arrivalPoints[activeIndex];
        if (activePoint == null || !activePoint.gameObject.activeSelf)
        {
            Debug.LogWarning("[Spawner] Cannot spawn drone — No active spawn point.");
            return null;
        }

        GameObject drone = dronePool.Get(activePoint.position, activePoint.rotation);
        drone.tag = "Drone";

        Debug.Log($"[Spawner] DRONE SPAWNED at index {activeIndex} — Pos: {activePoint.position}");

        return drone;
    }

    // ---------------- Utility ----------------
    ObjectPool CreatePool(GameObject prefab, string poolName)
    {
        GameObject obj = new GameObject(poolName);
        obj.transform.parent = this.transform;

        ObjectPool pool = obj.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialSize = 20;
        pool.expandable = true;

        return pool;
    }
}