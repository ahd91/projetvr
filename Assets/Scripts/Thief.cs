using UnityEngine;
using UnityEngine.AI;

// Voleur : il ignore le joueur, fonce vers le sapin, prend un cadeau
// puis s'enfuit avec vers son point d'apparition.
// S'il meurt avant de s'échapper, le cadeau retourne sous le sapin.
public class Thief : MonoBehaviour
{
    [Tooltip("Distance à son point de départ à partir de laquelle il s'est échappé")]
    [SerializeField] private float escapeRange = 1.5f;
    [Tooltip("Modèle de cadeau porté par le voleur (enfant désactivé, SANS collider)")]
    [SerializeField] private GameObject carriedGiftVisual;

    private NavMeshAgent agent;
    private ChristmasTree tree;
    private Vector3 exitPoint;     // il s'enfuit là où il est apparu
    private bool hasGift = false;  // porte-t-il un cadeau volé ?
    private bool leaving = false;  // a-t-il fini sa mission au sapin ?

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        exitPoint = transform.position;

        // Le sapin est trouvé par le nom de l'objet qui porte le script ChristmasTree
        GameObject treeObject = GameObject.Find("GiftZone");
        if (treeObject != null)
        {
            tree = treeObject.GetComponent<ChristmasTree>();
        }

        if (carriedGiftVisual != null)
        {
            carriedGiftVisual.SetActive(false);
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(name + " n'est pas sur la NavMesh");
            enabled = false;
        }
    }

    void Update()
    {
        if (tree == null)
        {
            return;
        }

        if (leaving == false)
        {
            // 1) Aller au sapin (le vol se fait quand il entre dans la zone : voir ReachTree)
            agent.SetDestination(tree.transform.position);
        }
        else
        {
            // 2) S'enfuir vers le point de départ
            agent.SetDestination(exitPoint);

            float distanceToExit = Vector3.Distance(transform.position, exitPoint);
            if (distanceToExit <= escapeRange)
            {
                if (hasGift)
                {
                    Debug.Log("Le voleur s'est échappé avec un cadeau...");
                }
                hasGift = false; // le cadeau est perdu : il ne doit pas revenir dans OnDestroy
                Destroy(gameObject);
            }
        }
    }

    // Appelée par le ChristmasTree quand le voleur entre dans la zone au pied du sapin
    public void ReachTree()
    {
        if (leaving)
        {
            return; // il a déjà fait son vol
        }

        // RemoveGift renvoie false s'il n'y avait aucun cadeau à voler
        hasGift = tree.RemoveGift();
        if (hasGift)
        {
            Debug.Log("Un voleur a pris un cadeau !");
            if (carriedGiftVisual != null)
            {
                carriedGiftVisual.SetActive(true);
            }
        }
        leaving = true;
    }

    // Appelée par Unity quand l'objet est détruit, quelle que soit la raison :
    // tué par le joueur ou disparu au lever du jour. S'il portait encore un cadeau, celui-ci revient sous le sapin.
    void OnDestroy()
    {
        if (hasGift && tree != null)
        {
            tree.AddGift(1);
            Debug.Log("Le cadeau volé est retourné sous le sapin !");
        }
    }
}
