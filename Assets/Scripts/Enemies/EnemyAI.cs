using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Detection Settings")]
    public LayerMask whatIsPlayer;
    public LayerMask whatIsGround;
    public float sightRange = 10f;
    public float groundCheckDistance = 2f;

    private bool playerInSightRange;
    private bool isGrounded;

    private void Awake()
    {
        // Get components
        agent = GetComponent<NavMeshAgent>();

        // Find player by name or tag
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("PlayerObj");
            if (playerObj == null)
                playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
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
        if (player == null || agent == null)
            return;

        // Ground Check
        Vector3 checkPos = transform.position + Vector3.down * 0.5f;
        isGrounded = Physics.CheckSphere(checkPos, 0.5f, whatIsGround);

        // Debug 
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, rayColor);


        // ground check
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        if (!isGrounded)
        {
            Debug.LogWarning($"{name} is NOT grounded! Ground layer mask: {whatIsGround.value}");
        }


        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} is NOT on NavMesh!");
            return;
        }

        //Player Detection 
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (playerInSightRange)
        {
            ChasePlayer();
            Debug.Log($"{name} → Player detected! Chasing {player.name}");
        }
        else
        {
            agent.ResetPath();
            Debug.Log($"{name} → Player not in range, idle");
        }
    }

    private void ChasePlayer()
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 checkPos = transform.position + Vector3.down * 0.5f;
        Gizmos.DrawWireSphere(checkPos, 0.5f);
    }


    private void OnDrawGizmosSelected()
    {
        // Sight range gizmo
        Gizmos.color = playerInSightRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Ground check line
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
