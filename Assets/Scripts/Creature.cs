using UnityEngine;
using UnityEngine.AI;

// Créature : suit le joueur sur la NavMesh, lui fait des dégâts quand elle est proche,
// et peut être ralentie par la capacité spéciale de l'arme.
public class Creature : MonoBehaviour
{
    [Header("Attaque")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackRange = 2f;     // distance à partir de laquelle elle frappe
    [SerializeField] private float attackCooldown = 1f;  // secondes entre deux coups

    [Header("Visuel")]
    [Tooltip("Le Renderer du modèle 3D (à glisser depuis le prefab). C'est lui qui devient cyan quand la créature est ralentie.")]
    [SerializeField] private Renderer rend;

    [Header("Ralentissement")]
    [SerializeField] private float slowFactor = 0.3f;    // 0.3 = 30 % de la vitesse normale

    private NavMeshAgent agent;
    private GameObject player;
    private Health playerHealth;

    private float normalSpeed;
    private Color normalColor;
    private float nextAttackTime = 0f; // moment où elle pourra frapper à nouveau
    private float slowEndTime = 0f;    // moment où le ralentissement s'arrête

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;

        // Si aucun Renderer n'est glissé dans l'Inspector, on prend celui de l'objet lui-même (la capsule)
        if (rend == null)
        {
            rend = GetComponent<Renderer>();
        }
        if (rend != null)
        {
            normalColor = rend.material.color;
        }

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

        // Vitesse et couleur selon si elle est ralentie ou non
        if (IsSlowed())
        {
            agent.speed = normalSpeed * slowFactor;
            if (rend != null)
            {
                rend.material.color = Color.cyan;
            }
        }
        else
        {
            agent.speed = normalSpeed;
            if (rend != null)
            {
                rend.material.color = normalColor;
            }
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

    // Appelée par le PlayerController quand on touche la créature au clic droit
    public void Slow(float duration)
    {
        slowEndTime = Time.time + duration;
    }

    // Servira pour le défi (ex : « tue 3 créatures ralenties »)
    public bool IsSlowed()
    {
        return Time.time < slowEndTime;
    }
}
