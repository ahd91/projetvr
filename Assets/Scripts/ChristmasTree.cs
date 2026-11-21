using UnityEngine;

// Le sapin : compte les cadeaux déposés à son pied et les affiche.
// À placer sur l'objet qui a le collider "Is Trigger" (la zone de dépôt autour du sapin).
public class ChristmasTree : MonoBehaviour
{
    [Tooltip("Nombre de cadeaux à réunir pour gagner")]
    [SerializeField] private int giftsToWin = 10;

    [Tooltip("Modèles de cadeaux posés sous le sapin, tous désactivés au départ. Le n-ième s'affiche quand on a n cadeaux.")]
    [SerializeField] private GameObject[] giftVisuals;

    private int giftCount = 0;
    private bool playerInZone = false;

    void Start()
    {
        UpdateVisuals();
    }

    // Le joueur entre dans la zone de dépôt (son CharacterController déclenche le trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
        }
    }

    // Le joueur sort de la zone
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    // Ajoute des cadeaux (value = 2 pour le cadeau doré, plus tard)
    public void AddGift(int value)
    {
        giftCount += value;
        Debug.Log("Cadeaux sous le sapin : " + giftCount + " / " + giftsToWin);
        UpdateVisuals();
    }

    // Retire un cadeau (servira aux voleurs). Renvoie false s'il n'y en avait plus.
    public bool RemoveGift()
    {
        if (giftCount <= 0)
        {
            return false;
        }
        giftCount--;
        UpdateVisuals();
        return true;
    }

    // Affiche autant de modèles de cadeaux qu'il y a de cadeaux sous le sapin
    private void UpdateVisuals()
    {
        for (int i = 0; i < giftVisuals.Length; i++)
        {
            giftVisuals[i].SetActive(i < giftCount);
        }
    }

    public bool IsPlayerInZone()
    {
        return playerInZone;
    }

    public int GetGiftCount()
    {
        return giftCount;
    }

    public int GetGiftsToWin()
    {
        return giftsToWin;
    }
}
