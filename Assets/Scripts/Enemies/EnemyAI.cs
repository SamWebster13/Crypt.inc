using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseWithGround : MonoBehaviour
{
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

    private void Update()
    {
        if (player == null || agent == null)
            return;

        // Check ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, whatIsGround);

        // Optional visual debug
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

        // Make sure the agent is on NavMesh and on the ground
        if (!agent.isOnNavMesh || !isGrounded)
            return;

        // Check if player is within detection range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

        if (playerInSightRange)
        {
            ChasePlayer();
        }
        else
        {
            // Stop moving when player is out of range
            agent.ResetPath();
        }
    }

    private void ChasePlayer()
    {
        // Move toward the player
        agent.SetDestination(player.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
