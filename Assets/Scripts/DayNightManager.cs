using UnityEngine;

// Gère l'alternance jour (Noël) / nuit (Halloween).
// Chaque phase dure un certain temps, puis on passe à l'autre.
public class DayNightManager : MonoBehaviour
{
    [Header("Durées (secondes)")]
    [SerializeField] private float dayDuration = 60f;
    [SerializeField] private float nightDuration = 60f;

    [Header("Lumière (glisser la Directional Light)")]
    [SerializeField] private Light sun;
    [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.85f);  // blanc chaud
    [SerializeField] private Color nightColor = new Color(0.4f, 0.5f, 1f);  // bleu froid
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.3f;

    [Header("Ciel (matériaux skybox)")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;

    [Header("Ambiance de nuit")]
    [Tooltip("Objet Global Volume avec le profil de post-traitement de nuit (activé seulement la nuit)")]
    [SerializeField] private GameObject nightVolume;
    [Tooltip("Couleur du brouillard : proche de la couleur de l'horizon du ciel de nuit")]
    [SerializeField] private Color fogColor = new Color(0.18f, 0.2f, 0.32f);
    [Tooltip("Lumière ambiante la nuit (1 = comme le jour)")]
    [SerializeField] private float nightAmbient = 0.4f;

    [Header("Décorations")]
    [Tooltip("Décorations visibles seulement le jour (les tas de neige)")]
    [SerializeField] private GameObject[] dayDecorations;
    [Tooltip("Décorations visibles seulement la nuit (les citrouilles, à la même place)")]
    [SerializeField] private GameObject[] nightDecorations;

    [Header("Cadeaux")]
    [Tooltip("Fait apparaître les cadeaux farceurs au début de chaque jour")]
    [SerializeField] private GiftSpawner giftSpawner;

    [Header("Défi de nuit")]
    [SerializeField] private ChallengeManager challengeManager;

    [Header("Monstres")]
    [Tooltip("Le Spawner n'est actif que la nuit")]
    [SerializeField] private Spawner spawner;

    private bool isNight = false;
    private float phaseEndTime = 0f; // moment où la phase actuelle se termine

    void Start()
    {
        StartDay();
    }

    void Update()
    {
        // La phase est terminée : on passe à l'autre
        if (Time.time >= phaseEndTime)
        {
            if (isNight)
            {
                StartDay();
            }
            else
            {
                StartNight();
            }
        }
    }

    private void StartDay()
    {
        isNight = false;
        phaseEndTime = Time.time + dayDuration;

        sun.color = dayColor;
        sun.intensity = dayIntensity;
        RenderSettings.skybox = daySkybox;
        RenderSettings.fog = false;
        RenderSettings.ambientIntensity = 1f;
        DynamicGI.UpdateEnvironment(); // recalcule la lumière ambiante avec le nouveau ciel
        nightVolume.SetActive(false);
        ShowDecorations(false);

        // Le jour, plus de monstres : on arrête le spawner et on fait disparaître ceux qui restent
        spawner.enabled = false;
        GameObject[] creatures = GameObject.FindGameObjectsWithTag("Creature");
        foreach (GameObject creature in creatures)
        {
            Destroy(creature);
        }

        giftSpawner.SpawnGifts();

        // Fin du défi de la nuit : s'il est réussi, un cadeau doré apparaît en plus
        if (challengeManager.EndChallenge())
        {
            giftSpawner.SpawnGoldenGift();
        }

        Debug.Log("Le jour se lève : c'est Noël !");
    }

    private void StartNight()
    {
        isNight = true;
        phaseEndTime = Time.time + nightDuration;

        sun.color = nightColor;
        sun.intensity = nightIntensity;
        RenderSettings.skybox = nightSkybox;
        RenderSettings.fog = true; // brouillard pour l'ambiance Halloween
        RenderSettings.fogColor = fogColor;
        RenderSettings.ambientIntensity = nightAmbient; // la scène est moins éclairée par le ciel
        DynamicGI.UpdateEnvironment();
        nightVolume.SetActive(true); // post-traitement de nuit : bloom, vignette, couleurs froides
        ShowDecorations(true);

        spawner.enabled = true;
        challengeManager.StartChallenge();

        // Les cadeaux farceurs pas attrapés disparaissent à la tombée de la nuit
        GameObject[] gifts = GameObject.FindGameObjectsWithTag("Gift");
        foreach (GameObject gift in gifts)
        {
            Destroy(gift);
        }

        Debug.Log("La nuit tombe : c'est Halloween !");
    }

    // Affiche les décorations de la phase : la neige le jour, les citrouilles la nuit
    private void ShowDecorations(bool night)
    {
        foreach (GameObject decoration in dayDecorations)
        {
            decoration.SetActive(night == false);
        }
        foreach (GameObject decoration in nightDecorations)
        {
            decoration.SetActive(night);
        }
    }

    // Utilisée par les autres scripts (HUD, arme...) pour savoir dans quelle phase on est
    public bool IsNight()
    {
        return isNight;
    }

    // Temps restant avant le changement de phase (pour le HUD)
    public float GetTimeLeft()
    {
        return phaseEndTime - Time.time;
    }
}
