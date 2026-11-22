using UnityEngine;

// Fait apparaître les cadeaux farceurs au début de chaque journée.
// Appelé par le DayNightManager.
public class GiftSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] giftPrefabs;   // plusieurs modèles possibles, un est tiré au hasard
    [SerializeField] private Transform[] spawnPoints;    // endroits possibles d'apparition
    [SerializeField] private int giftsPerDay = 4;

    public void SpawnGifts()
    {
        for (int i = 0; i < giftsPerDay; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = giftPrefabs[Random.Range(0, giftPrefabs.Length)];
            Instantiate(prefab, point.position, point.rotation);
        }
        Debug.Log(giftsPerDay + " cadeaux farceurs sont apparus !");
    }
}
