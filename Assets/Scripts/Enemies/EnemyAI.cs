using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Detection Settings")]
    public float sightRange = 15f; 

    private bool playerInSightRange;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInSightRange = distanceToPlayer <= sightRange;

        if (playerInSightRange)
        {
            ChasePlayer();
        }
        else
        {
            agent.ResetPath(); 
        }
    }

    private void ChasePlayer()
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
