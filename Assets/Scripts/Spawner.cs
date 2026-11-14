using UnityEngine;

// Fait apparaître une créature toutes les X secondes sur un point de spawn éloigné du joueur.
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] creaturePrefabs; // plusieurs formes possibles : une est tirée au hasard
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;     // secondes entre deux apparitions
    [SerializeField] private float minDistance = 15f;      // pas de spawn plus près du joueur (anti spawn kill)
    [SerializeField] private int maxCreatures = 10;        // limite pour ne pas faire ramer le PC

    private GameObject player;
    private float nextSpawnTime = 0f;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (player == null || Time.time < nextSpawnTime)
        {
            return;
        }
        nextSpawnTime = Time.time + spawnInterval;

        // Déjà trop de créatures en vie ? (il faut mettre le Tag "Creature" sur le prefab Creature)
        int nbCreatures = GameObject.FindGameObjectsWithTag("Creature").Length;
        if (nbCreatures >= maxCreatures)
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

        GameObject prefab = creaturePrefabs[Random.Range(0, creaturePrefabs.Length)];
        Instantiate(prefab, point.position, point.rotation);
    }
}
