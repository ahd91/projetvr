using System.Collections.Generic;
using UnityEngine;

// Fait apparaître une créature toutes les X secondes sur un point de spawn éloigné du joueur.
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] creaturePrefabs; // plusieurs formes possibles : une est tirée au hasard
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;   // secondes entre deux apparitions
    [SerializeField] private float minDistance = 15f;    // pas de spawn plus près du joueur (anti spawn kill)
    [SerializeField] private int maxCreatures = 10;      // ponytail: limite pour ne pas faire ramer le PC

    private Transform player;
    private float nextSpawnTime;
    private List<GameObject> alive = new List<GameObject>();

    void Start()
    {
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc)
        {
            player = pc.transform;
        }
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (!player || Time.time < nextSpawnTime)
        {
            return;
        }
        nextSpawnTime = Time.time + spawnInterval;

        alive.RemoveAll(c => c == null); // les créatures tuées sont détruites => null
        if (alive.Count >= maxCreatures || creaturePrefabs.Length == 0)
        {
            return;
        }

        // On garde seulement les points assez loin du joueur
        List<Transform> farPoints = new List<Transform>();
        foreach (Transform p in spawnPoints)
        {
            if (Vector3.Distance(p.position, player.position) >= minDistance)
            {
                farPoints.Add(p);
            }
        }
        if (farPoints.Count == 0)
        {
            return; // tous les points sont trop près : on réessaiera au prochain tour
        }

        Transform point = farPoints[Random.Range(0, farPoints.Count)];
        GameObject prefab = creaturePrefabs[Random.Range(0, creaturePrefabs.Length)];
        alive.Add(Instantiate(prefab, point.position, point.rotation));
    }
}
