using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject enemyPrefab;
    public GameObject dronePrefab;

    [Header("Spawn Settings")]
    public float enemySpawnInterval = 3f; // Time between enemy spawns

    private float enemyTimer;

    [Header("Spawn Points")]
    public Transform[] enemySpawnPoints;
    public Transform[] droneSpawnPoints;

    void Update()
    {
        HandleEnemySpawning();
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

    void SpawnEnemy()
    {
        if (enemySpawnPoints.Length == 0) return;

        Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    void SpawnDrone()
    {
        if (droneSpawnPoints.Length == 0) return;

        Transform spawnPoint = droneSpawnPoints[Random.Range(0, droneSpawnPoints.Length)];
        Instantiate(dronePrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
