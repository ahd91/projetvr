using UnityEngine;

// Défi de nuit : à chaque tombée de la nuit, un défi est tiré au hasard parmi trois.
// S'il est réussi, un cadeau doré apparaît le lendemain (géré par le DayNightManager).
public class ChallengeManager : MonoBehaviour
{
    [Header("Objectifs (tirés au hasard entre min et max)")]
    [SerializeField] private int minThieves = 2;
    [SerializeField] private int maxThieves = 3;
    [SerializeField] private int minCannonKills = 3;
    [SerializeField] private int maxCannonKills = 5;

    // 0 = ne perdre aucun cadeau, 1 = tuer X voleurs, 2 = tuer X créatures au canon
    private int challengeType;
    private int target;              // objectif à atteindre (types 1 et 2)
    private int progress;            // où on en est
    private bool active = false;     // un défi est-il en cours (= c'est la nuit) ?
    private bool succeeded = false;  // le défi de cette nuit est-il réussi ?
    private bool failed = false;     // pour le type 0 : un cadeau a-t-il été perdu ?
    private int score = 0;           // nombre de défis réussis depuis le début
    private bool rewardGolden = true; // récompense donnée : true = cadeau doré, false = vie restaurée

    // Appelée par le DayNightManager à la tombée de la nuit
    public void StartChallenge()
    {
        active = true;
        succeeded = false;
        failed = false;
        progress = 0;

        challengeType = Random.Range(0, 3); // 0, 1 ou 2
        if (challengeType == 1)
        {
            target = Random.Range(minThieves, maxThieves + 1); // +1 car le max est exclu
        }
        else if (challengeType == 2)
        {
            target = Random.Range(minCannonKills, maxCannonKills + 1);
        }

        Debug.Log("Défi de la nuit : " + GetChallengeText());
    }

    // Appelée par le DayNightManager au lever du jour. Renvoie true si le défi est réussi (=> cadeau doré)
    public bool EndChallenge()
    {
        if (active == false)
        {
            return false; // pas de défi en cours (ex : le tout premier jour)
        }
        active = false;

        // Pour le type 0, on ne sait qu'au lever du jour si aucun cadeau n'a été perdu
        if (challengeType == 0 && failed == false)
        {
            succeeded = true;
            score++;
        }
        return succeeded;
    }

    // Appelée par Health quand un voleur meurt
    public void ThiefKilled()
    {
        if (active && challengeType == 1)
        {
            AddProgress();
        }
    }

    // Appelée par le PlayerController quand le canon tue une créature
    public void CannonKill()
    {
        if (active && challengeType == 2)
        {
            AddProgress();
        }
    }

    // Appelée par un voleur qui s'échappe avec un cadeau
    public void GiftLost()
    {
        if (active && challengeType == 0)
        {
            failed = true;
            Debug.Log("Défi raté : un voleur s'est enfui avec un cadeau");
        }
    }

    private void AddProgress()
    {
        if (succeeded)
        {
            return; // déjà réussi cette nuit
        }
        progress++;
        if (progress >= target)
        {
            succeeded = true;
            score++;
            Debug.Log("Défi réussi ! Un cadeau doré apparaîtra demain.");
        }
    }

    // Texte affiché par le HUD
    public string GetChallengeText()
    {
        // Le jour : on rappelle le résultat de la nuit précédente
        if (active == false)
        {
            if (succeeded && rewardGolden)
            {
                return "Défi de la nuit réussi : attrape le cadeau doré (il vaut 2) !";
            }
            if (succeeded)
            {
                return "Défi de la nuit réussi : tu as récupéré 30 % de ta vie !";
            }
            return "Défi : il sera tiré à la tombée de la nuit";
        }

        // La nuit : le défi en cours
        if (challengeType == 0)
        {
            if (failed)
            {
                return "Défi raté : un voleur s'est enfui avec un cadeau";
            }
            return "Défi : ne laisse aucun voleur s'enfuir avec un cadeau";
        }

        string text;
        if (challengeType == 1)
        {
            text = "Défi : tue " + target + " voleurs (" + progress + "/" + target + ")";
        }
        else
        {
            text = "Défi : tue " + target + " créatures au canon (" + progress + "/" + target + ")";
        }
        if (succeeded)
        {
            text = text + " - RÉUSSI !";
        }
        return text;
    }

    // Appelée par le DayNightManager pour dire quelle récompense a été donnée
    public void SetReward(bool golden)
    {
        rewardGolden = golden;
    }

    public int GetScore()
    {
        return score;
    }
}
