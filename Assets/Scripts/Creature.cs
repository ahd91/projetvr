using UnityEngine;
using UnityEngine.AI;

// Créature (chasseur) : suit le joueur sur la NavMesh et lui fait des dégâts quand elle est proche.
public class Creature : MonoBehaviour
{
    [Header("Attaque")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 2f;     // distance à partir de laquelle elle frappe
    [SerializeField] private float attackCooldown = 1f;  // secondes entre deux coups

    private NavMeshAgent agent;
    private GameObject player;
    private Health playerHealth;

    private float nextAttackTime = 0f; // moment où elle pourra frapper à nouveau

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Le joueur est trouvé par son Tag (il faut mettre le Tag "Player" sur le prefab Player)
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        // Hors NavMesh, SetDestination fait une erreur à CHAQUE frame : on prévient une fois et on s'arrête
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(name + " n'est pas sur la NavMesh");
            enabled = false;
        }
    }

    void Update()
    {
        // Joueur mort (détruit) : on ne fait plus rien
        if (player == null)
        {
            return;
        }

        // Suivre le joueur
        agent.SetDestination(player.transform.position);

        // Frapper si on est assez proche, qu'aucun mur ne nous sépare, et que le cooldown est fini.
        // On vise le centre du joueur : son pivot est à ses pieds, alors que celui de la créature est en son milieu.
        Vector3 playerCenter = player.transform.position + Vector3.up;
        float distance = Vector3.Distance(transform.position, playerCenter);
        if (distance <= attackRange && Time.time >= nextAttackTime && CanSeePlayer(playerCenter))
        {
            nextAttackTime = Time.time + attackCooldown;
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    // Lance un rayon vers le joueur : true si le premier objet touché est bien le joueur (pas un mur)
    private bool CanSeePlayer(Vector3 playerCenter)
    {
        Vector3 direction = playerCenter - transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, attackRange))
        {
            return hit.collider.gameObject == player;
        }
        return false;
    }
}
