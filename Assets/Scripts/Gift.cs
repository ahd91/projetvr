using UnityEngine;
using UnityEngine.AI;

// Cadeau farceur : il s'enfuit quand le joueur s'approche.
// Il faut le ralentir (clic droit) pour pouvoir l'attraper (touche E).
public class Gift : MonoBehaviour
{
    [Tooltip("Nombre de cadeaux qu'il rapporte (2 pour le cadeau doré)")]
    [SerializeField] private int value = 1;

    [Header("Fuite")]
    [Tooltip("Distance à partir de laquelle il s'enfuit")]
    [SerializeField] private float fleeDistance = 8f;
    [Tooltip("Jusqu'où il essaie de courir à chaque fois")]
    [SerializeField] private float fleeStep = 6f;

    [Header("Ralentissement")]
    [SerializeField] private float slowFactor = 0.3f; // 0.3 = 30 % de la vitesse normale
    [Tooltip("Le Renderer du modèle (à glisser) : il devient cyan quand le cadeau est ralenti")]
    [SerializeField] private Renderer rend;

    private NavMeshAgent agent;
    private GameObject player;
    private float normalSpeed;
    private Color normalColor;
    private float slowEndTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;
        if (rend != null)
        {
            normalColor = rend.material.color;
        }

        player = GameObject.FindWithTag("Player");

        // Même sécurité que pour les créatures : hors NavMesh, SetDestination ferait une erreur à chaque frame
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(name + " n'est pas sur la NavMesh");
            enabled = false;
        }
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        // Vitesse et couleur selon si le cadeau est ralenti ou non
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

        // Le joueur est trop près : on court dans la direction opposée
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance < fleeDistance)
        {
            Vector3 away = transform.position - player.transform.position; // vecteur joueur -> cadeau
            away.y = 0;                                                    // on reste à plat
            Vector3 target = transform.position + away.normalized * fleeStep;
            agent.SetDestination(target);
        }
    }

    // Appelée par le PlayerController (clic droit, le jour)
    public void Slow(float duration)
    {
        slowEndTime = Time.time + duration;
    }

    public bool IsSlowed()
    {
        return Time.time < slowEndTime;
    }

    public int GetValue()
    {
        return value;
    }
}
