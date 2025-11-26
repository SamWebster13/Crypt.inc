using UnityEngine;

public class Spawner : MonoBehaviour
{
[Header("Prefabs (same as your original script)")]
public GameObject enemyPrefab;
public GameObject dronePrefab;

[Header("Spawn Settings")]  
public float enemySpawnInterval = 3f;  
public float enemyTimer;  

[Header("Spawn Points")]  
public Transform[] enemySpawnPoints;  
public Transform[] droneSpawnPoints;  

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

    // Make sure spawned enemy has the "Enemy" tag  
    enemy.tag = "Enemy";  

    return enemy;  
}  

public GameObject SpawnDrone()  
{  
    if (droneSpawnPoints.Length == 0) return null;  

    Transform point = droneSpawnPoints[Random.Range(0, droneSpawnPoints.Length)];  
    return dronePool.Get(point.position, point.rotation);  
}  

void KillAllEnemies()  
{  
    // Find all GameObjects with the "Enemy" tag and destroy them
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  
    foreach (var enemy in enemies)  
    {  
        Destroy(enemy); // destroys the GameObject  
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
