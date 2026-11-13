using UnityEngine;
using UnityEngine.AI;

// Créature : suit le joueur sur la NavMesh et lui fait des dégâts quand elle est collée à lui.
[RequireComponent(typeof(NavMeshAgent))]
public class Creature : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f; // secondes entre deux coups

    private NavMeshAgent agent;
    private Transform player;
    private Health playerHealth;
    private float nextAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // ponytail: un seul joueur dans la scène, pas besoin de tag
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc)
        {
            player = pc.transform;
            playerHealth = pc.GetComponent<Health>();
        }

        // Hors NavMesh, SetDestination lance une erreur à CHAQUE frame (=> 999+ erreurs).
        // On prévient une seule fois et on désactive le script.
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(name + " n'est pas sur la NavMesh : re-Bake la NavMesh ou rapproche la créature du sol.");
            enabled = false;
        }
    }

    void Update()
    {
        if (!player)
        {
            return; // joueur mort (détruit) : on ne fait plus rien
        }

        agent.SetDestination(player.position);

        // Distance à plat (on ignore la hauteur : les pivots ne sont pas au même niveau)
        Vector3 diff = player.position - transform.position;
        diff.y = 0;
        if (diff.magnitude <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            if (playerHealth)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
