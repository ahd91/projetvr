using UnityEngine;
using UnityEngine.AI;

// Fait apparaître les cadeaux farceurs au début de chaque journée.
// Appelé par le DayNightManager.
// Plus il y a de cadeaux sous le sapin, plus les nouveaux cadeaux sont rapides.
public class GiftSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] giftPrefabs;   // plusieurs modèles possibles, un est tiré au hasard
    [SerializeField] private Transform[] spawnPoints;    // endroits possibles d'apparition
    [SerializeField] private int giftsPerDay = 4;
    [Tooltip("Cadeau doré : récompense du défi de nuit (vaut 2, plus rapide)")]
    [SerializeField] private GameObject goldenGiftPrefab;

    [Header("Difficulté")]
    [Tooltip("Le sapin, pour connaître le nombre de cadeaux (glisser GiftZone)")]
    [SerializeField] private ChristmasTree tree;
    [Tooltip("Vitesse ajoutée aux cadeaux pour chaque cadeau déjà sous le sapin")]
    [SerializeField] private float speedPerGift = 0.2f;

    public void SpawnGifts()
    {
        float bonusSpeed = speedPerGift * tree.GetGiftCount();

        for (int i = 0; i < giftsPerDay; i++)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = giftPrefabs[Random.Range(0, giftPrefabs.Length)];
            GameObject gift = Instantiate(prefab, point.position, point.rotation);

            // On accélère le cadeau qui vient d'apparaître (avant son Start, qui mémorise sa vitesse normale)
            NavMeshAgent agent = gift.GetComponent<NavMeshAgent>();
            agent.speed = agent.speed + bonusSpeed;
        }
        Debug.Log(giftsPerDay + " cadeaux farceurs sont apparus (vitesse +" + bonusSpeed + ")");
    }

    // Appelée par le DayNightManager quand le défi de la nuit a été réussi
    public void SpawnGoldenGift()
    {
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject gift = Instantiate(goldenGiftPrefab, point.position, point.rotation);

        NavMeshAgent agent = gift.GetComponent<NavMeshAgent>();
        agent.speed = agent.speed + speedPerGift * tree.GetGiftCount();
        Debug.Log("Un cadeau doré est apparu !");
    }
}
