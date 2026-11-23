using UnityEngine;

// Points de vie : sert pour les créatures et pour le joueur.
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [Tooltip("Détruire l'objet à 0 PV ? (à décocher pour le joueur : c'est le HUD qui affiche le game over)")]
    [SerializeField] private bool destroyOnDeath = true;
    [Tooltip("Effet (fumée) créé à la mort de l'objet. Peut rester vide.")]
    [SerializeField] private GameObject deathEffect;
    private int hp;

    void Start()
    {
        hp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        // Déjà mort (Destroy n'agit qu'en fin de frame) : on ignore les coups suivants
        if (hp <= 0)
        {
            return;
        }
        hp -= amount;
        Debug.Log(name + " : " + hp + " PV");
        if (hp <= 0)
        {
            // Si c'est un voleur, on prévient le gestionnaire de défi
            Thief thief = GetComponent<Thief>();
            GameObject manager = GameObject.Find("ChallengeManager");
            if (thief != null && manager != null)
            {
                manager.GetComponent<ChallengeManager>().ThiefKilled();
            }

            if (destroyOnDeath)
            {
                if (deathEffect != null)
                {
                    GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
                    Destroy(effect, 3f);
                }
                Destroy(gameObject);
            }
        }
    }

    // Utilisée par le HUD pour afficher la vie
    public int GetHp()
    {
        return hp;
    }

    // Utilisée par la barre de vie (valeur maximale) et pour savoir si le joueur a toute sa vie
    public int GetMaxHp()
    {
        return maxHp;
    }

    // Rend des PV, sans dépasser la vie maximale (récompense du défi de nuit quand le joueur est blessé)
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp); // Mathf.Min : on garde la plus petite des deux valeurs
        Debug.Log(name + " : +" + amount + " PV (" + hp + " PV)");
    }
}
