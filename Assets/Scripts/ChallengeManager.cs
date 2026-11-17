using UnityEngine;

// Gère le défi en cours et le score.
// Un défi = tuer un certain nombre de créatures (normales ou ralenties).
// Quand le défi est réussi : score + 1 et on génère un nouveau défi.
public class ChallengeManager : MonoBehaviour
{
    [Tooltip("Nombre minimum de créatures à tuer pour un défi")]
    [SerializeField] private int minTarget = 2;
    [Tooltip("Nombre maximum de créatures à tuer pour un défi")]
    [SerializeField] private int maxTarget = 5;

    private int challengeType; // 0 = tuer des créatures, 1 = tuer des créatures RALENTIES
    private int target;        // combien il faut en tuer
    private int progress;      // combien on en a déjà tué
    private int score = 0;

    void Start()
    {
        NewChallenge();
    }

    // Tire un nouveau défi au hasard
    private void NewChallenge()
    {
        challengeType = Random.Range(0, 2);              // 0 ou 1
        target = Random.Range(minTarget, maxTarget + 1); // +1 car le max est exclu
        progress = 0;
        Debug.Log("Nouveau défi : " + GetChallengeText());
    }

    // Appelée par Health quand une créature meurt
    public void CreatureKilled(bool wasSlowed)
    {
        // Pour le défi "ralenties", une créature normale ne compte pas
        if (challengeType == 1 && wasSlowed == false)
        {
            Debug.Log("Créature non ralentie : elle ne compte pas pour ce défi");
            return;
        }

        progress++;
        Debug.Log(GetChallengeText());

        if (progress >= target)
        {
            score++;
            Debug.Log("Défi réussi ! Score : " + score);
            NewChallenge();
        }
    }

    // Texte du défi (servira aussi pour l'UI à l'étape 7)
    public string GetChallengeText()
    {
        if (challengeType == 0)
        {
            return "Tue " + target + " créatures (" + progress + "/" + target + ")";
        }
        else
        {
            return "Tue " + target + " créatures ralenties (" + progress + "/" + target + ")";
        }
    }

    public int GetScore()
    {
        return score;
    }
}
