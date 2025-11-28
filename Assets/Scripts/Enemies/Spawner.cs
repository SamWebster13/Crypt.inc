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
    public Transform[] droneSpawnPoints;  // Only active ones will be used

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
        if (cycle != null)  
        {  
            cycle.onSunrise.AddListener(KillAllEnemies); // clear enemies at day  
        }  
    }  

    void Update()  
    {  
        if (cycle != null && cycle.isNight)  
        {  
            HandleEnemySpawning();  
        }  

        HandleDroneSpawning();  
    }  

    void HandleEnemySpawning()  
    {  
        enemyTimer += Time.deltaTime;  

        if (enemyTimer >= enemySpawnInterval)  
        {  
            enemyTimer = 0f;  
            SpawnEnemy();  
        }  
    }  

    void HandleDroneSpawning()  
    {  
        // Spawn drones on input for testing, or you can call this automatically
        if (Input.GetKeyDown(KeyCode.F))  
        {  
            SpawnDrone();  
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

    public GameObject SpawnDrone()  
    {  
        // Only use active drone spawn points
        Transform[] activePoints = System.Array.FindAll(droneSpawnPoints, p => p != null && p.gameObject.activeInHierarchy);

        if (activePoints.Length == 0) return null;  

        Transform point = activePoints[Random.Range(0, activePoints.Length)];  
        GameObject drone = dronePool.Get(point.position, point.rotation);  

        drone.tag = "Drone";  

        return drone;  
    }  

    void KillAllEnemies()  
    {  
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  
        foreach (var enemy in enemies)  
        {  
            Destroy(enemy);  
        }  
    }  

    ObjectPool CreatePool(GameObject prefab, string poolName)  
    {  
        GameObject poolObj = new GameObject(poolName);  
        poolObj.transform.parent = this.transform;  

        ObjectPool pool = poolObj.AddComponent<ObjectPool>();  
        pool.prefab = prefab;  
        pool.initialSize = 20;  
        pool.expandable = true;  

        return pool;  
    }  
}
