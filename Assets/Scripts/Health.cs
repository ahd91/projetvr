using UnityEngine;

// Points de vie : sert pour les créatures (et plus tard pour le joueur).
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
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
            // Si c'est une créature, on prévient le gestionnaire de défi
            Creature creature = GetComponent<Creature>();
            GameObject manager = GameObject.Find("ChallengeManager");
            if (creature != null && manager != null)
            {
                manager.GetComponent<ChallengeManager>().CreatureKilled(creature.IsSlowed());
            }

            Destroy(gameObject);
        }
    }
}
