using UnityEngine;

// Fait apparaître des créatures la nuit sur un point de spawn éloigné du joueur.
// Une partie des créatures sont des voleurs (ils vont au sapin), les autres des chasseurs (ils attaquent le joueur).
// La difficulté dépend du nombre de cadeaux sous le sapin : plus il y en a, plus c'est dur.
public class Spawner : MonoBehaviour
{
    [Header("Prefabs et points")]
    [SerializeField] private GameObject[] creaturePrefabs; // chasseurs : plusieurs formes possibles, une est tirée au hasard
    [SerializeField] private GameObject[] thiefPrefabs;    // voleurs
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minDistance = 15f;      // pas de spawn plus près du joueur (anti spawn kill)

    [Header("Difficulté (n = nombre de cadeaux sous le sapin)")]
    [Tooltip("Le sapin, pour connaître n (glisser GiftZone)")]
    [SerializeField] private ChristmasTree tree;
    [Tooltip("Créatures max = baseMaxCreatures + n")]
    [SerializeField] private int baseMaxCreatures = 3;
    [Tooltip("Intervalle = baseInterval - intervalPerGift x n, sans descendre sous minInterval")]
    [SerializeField] private float baseInterval = 4f;
    [SerializeField] private float intervalPerGift = 0.2f;
    [SerializeField] private float minInterval = 1.5f;
    [Tooltip("Probabilité d'un voleur = baseThiefChance + thiefChancePerGift x n, sans dépasser maxThiefChance")]
    [SerializeField] private float baseThiefChance = 0.2f;
    [SerializeField] private float thiefChancePerGift = 0.05f;
    [SerializeField] private float maxThiefChance = 0.8f;

    private GameObject player;
    private float nextSpawnTime = 0f;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Appelée par Unity quand le script est activé (le DayNightManager l'active à la tombée de la nuit)
    void OnEnable()
    {
        if (tree != null)
        {
            Debug.Log("Nuit avec " + tree.GetGiftCount() + " cadeaux : max " + GetMaxCreatures()
                + " créatures, une toutes les " + GetInterval() + " s, " + (GetThiefChance() * 100) + " % de voleurs");
        }
    }

    void Update()
    {
        if (player == null || Time.time < nextSpawnTime)
        {
            return;
        }
        nextSpawnTime = Time.time + GetInterval();

        // Déjà trop de créatures en vie ? (chasseurs et voleurs ont tous le Tag "Creature")
        int nbCreatures = GameObject.FindGameObjectsWithTag("Creature").Length;
        if (nbCreatures >= GetMaxCreatures())
        {
            return;
        }

        // On tire un point au hasard. S'il est trop près du joueur, on ne fait rien :
        // on retentera au prochain tour avec un autre point.
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        float distance = Vector3.Distance(point.position, player.transform.position);
        if (distance < minDistance)
        {
            return;
        }

        // Random.value donne un nombre au hasard entre 0 et 1 : en dessous de la probabilité, c'est un voleur
        GameObject prefab;
        if (Random.value < GetThiefChance())
        {
            prefab = thiefPrefabs[Random.Range(0, thiefPrefabs.Length)];
        }
        else
        {
            prefab = creaturePrefabs[Random.Range(0, creaturePrefabs.Length)];
        }
        Instantiate(prefab, point.position, point.rotation);
    }

    // ----- Calcul de la difficulté à partir du nombre de cadeaux -----

    private int GetMaxCreatures()
    {
        return baseMaxCreatures + tree.GetGiftCount();
    }

    private float GetInterval()
    {
        float interval = baseInterval - intervalPerGift * tree.GetGiftCount();
        return Mathf.Max(interval, minInterval); // Mathf.Max garde la plus grande des deux valeurs
    }

    private float GetThiefChance()
    {
        float chance = baseThiefChance + thiefChancePerGift * tree.GetGiftCount();
        return Mathf.Min(chance, maxThiefChance); // Mathf.Min garde la plus petite des deux valeurs
    }
}
