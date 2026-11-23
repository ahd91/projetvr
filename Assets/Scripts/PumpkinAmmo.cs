using UnityEngine;

// Citrouille de nuit qui sert de munition au canon.
// Le joueur la vise et appuie sur E : il gagne une charge de canon et la citrouille disparaît.
// Elle réapparaît à la nuit suivante (c'est le DayNightManager qui réactive les décorations de nuit).
public class PumpkinAmmo : MonoBehaviour
{
    [Tooltip("Nombre de charges de canon données par cette citrouille")]
    [SerializeField] private int charges = 1;

    // Appelée par le PlayerController quand le joueur ramasse la citrouille. Renvoie le nombre de charges gagnées.
    public int Collect()
    {
        gameObject.SetActive(false);
        return charges;
    }
}
