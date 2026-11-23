using UnityEngine;

// Effets visuels de l'arme (sucre d'orge magique) : trait lumineux, impacts, explosion et recul.
// Appelé par le PlayerController à chaque tir.
public class WeaponEffects : MonoBehaviour
{
    [Header("Trait du tir (LineRenderer)")]
    [SerializeField] private LineRenderer line;
    [Tooltip("Point de départ du trait : le bout du sucre d'orge")]
    [SerializeField] private Transform muzzle;
    [Tooltip("Durée d'affichage du trait (secondes)")]
    [SerializeField] private float lineDuration = 0.06f;
    [SerializeField] private Color shotColor = Color.white;
    [SerializeField] private Color slowColor = Color.cyan;
    [SerializeField] private Color cannonColor = new Color(1f, 0.5f, 0.1f); // orange

    [Header("Particules")]
    [Tooltip("Étincelles au point d'impact")]
    [SerializeField] private GameObject impactPrefab;
    [Tooltip("Explosion du canon")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Animation")]
    [Tooltip("Animator qui joue le recul (paramètre Trigger « Fire »)")]
    [SerializeField] private Animator animator;

    private float lineEndTime = 0f; // moment où le trait doit disparaître

    void Start()
    {
        line.enabled = false;
    }

    void Update()
    {
        // Le trait ne reste affiché qu'un court instant
        if (line.enabled && Time.time >= lineEndTime)
        {
            line.enabled = false;
        }
    }

    // Tir normal (clic gauche)
    public void PlayShot(Vector3 end, bool touched)
    {
        DrawLine(end, shotColor, 0.04f);
        if (touched)
        {
            SpawnEffect(impactPrefab, end, 1f);
        }
    }

    // Ralentisseur (clic droit le jour)
    public void PlaySlow(Vector3 end, bool touched)
    {
        DrawLine(end, slowColor, 0.06f);
        if (touched)
        {
            SpawnEffect(impactPrefab, end, 1f);
        }
    }

    // Canon (clic droit la nuit)
    public void PlayCannon(Vector3 end, bool touched)
    {
        DrawLine(end, cannonColor, 0.15f);
        if (touched)
        {
            SpawnEffect(explosionPrefab, end, 3f);
        }
    }

    // Affiche un trait du bout de l'arme jusqu'au point visé, et lance l'animation de recul
    private void DrawLine(Vector3 end, Color color, float width)
    {
        line.SetPosition(0, muzzle.position);
        line.SetPosition(1, end);
        line.startColor = color;
        line.endColor = color;
        line.widthMultiplier = width;
        line.enabled = true;
        lineEndTime = Time.time + lineDuration;

        if (animator != null)
        {
            animator.SetTrigger("Fire");
        }
    }

    // Crée un effet de particules à un endroit, puis le détruit au bout de "lifetime" secondes
    private void SpawnEffect(GameObject prefab, Vector3 position, float lifetime)
    {
        if (prefab == null)
        {
            return;
        }
        GameObject effect = Instantiate(prefab, position, Quaternion.identity);
        Destroy(effect, lifetime);
    }
}
