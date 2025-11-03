using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class HunterAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;

    [Header("Detection Settings")]
    public string enemyTag = "Enemy";         // What tag the hunter looks for
    public LayerMask whatIsGround;
    public float detectionRange = 15f;        // How far it can detect enemies
    public float attackRange = 2f;            // How close it must be to kill
    public float groundCheckDistance = 2f;

    private Transform currentTarget;
    private bool isGrounded;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Snap to NavMesh if slightly above or below
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void Update()
    {
        // --- Ground Check ---
        Vector3 checkPos = transform.position + Vector3.down * 0.5f;
        isGrounded = Physics.CheckSphere(checkPos, 0.5f, whatIsGround);

        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        if (!isGrounded)
        {
            Debug.LogWarning($"{name} is NOT grounded!");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is NOT on NavMesh!");
            return;
        }

        // --- Find Nearest Enemy ---
        currentTarget = FindNearestEnemy();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);

            // Move toward target
            agent.SetDestination(currentTarget.position);

            Debug.Log($"{name} → Targeting {currentTarget.name}, Distance: {distance:F2}");

            // Attack / Kill if close enough
            if (distance <= attackRange)
            {
                KillEnemy(currentTarget.gameObject);
            }
        }
        else
        {
            agent.ResetPath();
            Debug.Log($"{name} → No enemies in range");
        }
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0)
            return null;

        GameObject nearest = enemies
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .FirstOrDefault();

        float distance = Vector3.Distance(transform.position, nearest.transform.position);
        if (distance <= detectionRange)
            return nearest.transform;
        else
            return null;
    }

    private void KillEnemy(GameObject enemy)
    {
        Debug.Log($"{name} → Killed {enemy.name}!");
        Destroy(enemy);
    }

    private void OnDrawGizmos()
    {
        // Ground check sphere
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 checkPos = transform.position + Vector3.down * 0.5f;
        Gizmos.DrawWireSphere(checkPos, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
