using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class HunterAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    private Rigidbody rb;

    [Header("Detection Settings")]
    public string enemyTag = "Enemy";
    public float detectionRange = 15f;
    public float attackRange = 2f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;     // gravity strength
    public float groundCheckDistance = 0.5f;
    public LayerMask whatIsGround;

    private Transform currentTarget;
    private bool isGrounded;
    private Vector3 velocity;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        // Add Rigidbody if missing
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody so NavMeshAgent can still work
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false; // we'll apply gravity manually
    }

    private void Update()
    {
        GroundCheck();

        // Apply gravity if not grounded
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            rb.MovePosition(transform.position + velocity * Time.deltaTime);
        }
        else
        {
            velocity.y = 0f;
        }

        if (!agent.isOnNavMesh)
            return;

        currentTarget = FindNearestEnemy();

        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            agent.SetDestination(currentTarget.position);

            if (distance <= attackRange)
            {
                KillEnemy(currentTarget.gameObject);
            }
        }
        else
        {
            agent.ResetPath();
        }

        // Keep agent and Rigidbody synced
        agent.nextPosition = transform.position;
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, whatIsGround);
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
        return distance <= detectionRange ? nearest.transform : null;
    }

    private void KillEnemy(GameObject enemy)
    {
        Destroy(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
