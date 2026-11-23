using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Affiche la vie (barre), le score, le défi, la phase, les cadeaux et l'arme.
// Fait clignoter l'écran en rouge quand le joueur est touché, et annonce les changements de phase en grand.
// Affiche le game over quand le joueur n'a plus de vie, et la victoire à 10 cadeaux.
public class HUD : MonoBehaviour
{
    [Header("Vie")]
    [Tooltip("Barre de vie (UI > Slider)")]
    [SerializeField] private Slider healthBar;
    [Tooltip("Image rouge plein écran, transparente au repos : elle clignote quand le joueur est touché")]
    [SerializeField] private Image damageFlash;
    [SerializeField] private float flashFadeSpeed = 3f; // vitesse à laquelle le rouge disparaît

    [Header("Grand texte au centre (changement de phase)")]
    [SerializeField] private Text bigText;
    [Tooltip("Compte à rebours affiché pendant les X dernières secondes de chaque phase")]
    [SerializeField] private float countdownSeconds = 5f;
    [Tooltip("Durée du titre affiché au début de chaque phase")]
    [SerializeField] private float titleSeconds = 2.5f;

    [Header("Textes (UI > Legacy > Text)")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text challengeText;
    [SerializeField] private Text phaseText;
    [SerializeField] private Text giftText;
    [SerializeField] private Text weaponText;

    [Header("Game over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Victoire")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Références (glisser depuis la Hierarchy)")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private DayNightManager dayNightManager;
    [SerializeField] private ChristmasTree tree;
    [SerializeField] private PlayerController player;

    private int lastHp; // vie à la frame précédente, pour savoir si le joueur vient d'être touché

    void Start()
    {
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        lastHp = playerHealth.GetHp();
    }

    void Update()
    {
        UpdateHealth();
        UpdateBigText();
        scoreText.text = "Défis réussis : " + challengeManager.GetScore();
        challengeText.text = challengeManager.GetChallengeText();

        // Phase en cours + secondes restantes (arrondies au-dessus pour ne jamais afficher 0 trop tôt)
        int secondsLeft = Mathf.CeilToInt(dayNightManager.GetTimeLeft());
        if (dayNightManager.IsNight())
        {
            phaseText.text = "Nuit d'Halloween : encore " + secondsLeft + " s";
        }
        else
        {
            phaseText.text = "Jour de Noël : encore " + secondsLeft + " s";
        }

        giftText.text = "Cadeaux sous le sapin : " + tree.GetGiftCount() + " / " + tree.GetGiftsToWin();
        if (player.IsCarryingGift())
        {
            giftText.text = giftText.text + "   (tu portes un cadeau : va au sapin et appuie sur E)";
        }

        // Arme du clic droit : ralentisseur le jour, canon la nuit (avec sa recharge)
        if (dayNightManager.IsNight())
        {
            int charges = player.GetCannonCharges();
            float cannonTimeLeft = player.GetCannonTimeLeft();
            if (charges <= 0)
            {
                weaponText.text = "Canon : plus de munitions, ramasse une citrouille (E)";
            }
            else if (cannonTimeLeft > 0)
            {
                weaponText.text = "Canon : " + charges + " charge(s), recharge " + Mathf.CeilToInt(cannonTimeLeft) + " s";
            }
            else
            {
                weaponText.text = "Canon : " + charges + " charge(s), PRÊT (clic droit)";
            }
        }
        else
        {
            weaponText.text = "Ralentisseur (clic droit)";
        }

        // Assez de cadeaux et la victoire n'est pas encore affichée
        if (tree.GetGiftCount() >= tree.GetGiftsToWin() && victoryPanel.activeSelf == false)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Le joueur est mort et le game over n'est pas encore affiché
        if (playerHealth.GetHp() <= 0 && gameOverPanel.activeSelf == false)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;                      // met le jeu en pause (le temps ne s'écoule plus)
            Cursor.lockState = CursorLockMode.None;  // libère la souris pour cliquer sur le bouton
            Cursor.visible = true;
        }
    }

    // Barre de vie + flash rouge quand la vie baisse
    private void UpdateHealth()
    {
        int hp = playerHealth.GetHp();
        healthBar.maxValue = playerHealth.GetMaxHp();
        healthBar.value = hp;

        // La vie a baissé depuis la frame précédente : l'écran devient rouge d'un coup...
        Color c = damageFlash.color;
        if (hp < lastHp)
        {
            c.a = 0.45f;
        }
        // ... puis le rouge disparaît petit à petit
        c.a = Mathf.MoveTowards(c.a, 0f, flashFadeSpeed * Time.deltaTime);
        damageFlash.color = c;
        lastHp = hp;
    }

    // Grand texte au centre : titre au début de la phase, compte à rebours à la fin
    private void UpdateBigText()
    {
        float timeLeft = dayNightManager.GetTimeLeft();
        if (dayNightManager.GetPhaseElapsed() < titleSeconds)
        {
            if (dayNightManager.IsNight())
            {
                bigText.text = "LA NUIT D'HALLOWEEN COMMENCE !";
                bigText.color = new Color(1f, 0.5f, 0.1f); // orange
            }
            else
            {
                bigText.text = "JOUR DE NOËL !";
                bigText.color = new Color(1f, 0.95f, 0.6f); // blanc doré
            }
        }
        else if (timeLeft < countdownSeconds)
        {
            int seconds = Mathf.CeilToInt(timeLeft);
            if (dayNightManager.IsNight())
            {
                bigText.text = "Le jour se lève dans " + seconds + "...";
            }
            else
            {
                bigText.text = "La nuit tombe dans " + seconds + "...";
            }
            bigText.color = Color.white;
        }
        else
        {
            bigText.text = "";
        }
    }

    // Appelée par le bouton "Rejouer" (événement OnClick, comme dans le cours V)
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
