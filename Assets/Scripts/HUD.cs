using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Affiche la vie, le score et le défi en cours. Affiche le game over quand le joueur n'a plus de vie.
public class HUD : MonoBehaviour
{
    [Header("Textes (UI > Legacy > Text)")]
    [SerializeField] private Text healthText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text challengeText;

    [Header("Game over")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Références (glisser depuis la Hierarchy)")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private ChallengeManager challengeManager;

    void Start()
    {
        gameOverPanel.SetActive(false);
    }

    void Update()
    {
        healthText.text = "Vie : " + playerHealth.GetHp();
        scoreText.text = "Score : " + challengeManager.GetScore();
        challengeText.text = "Défi : " + challengeManager.GetChallengeText();

        // Le joueur est mort et le game over n'est pas encore affiché
        if (playerHealth.GetHp() <= 0 && gameOverPanel.activeSelf == false)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0;                      // met le jeu en pause (le temps ne s'écoule plus)
            Cursor.lockState = CursorLockMode.None;  // libère la souris pour cliquer sur le bouton
            Cursor.visible = true;
        }
    }

    // Appelée par le bouton "Rejouer" (événement OnClick, comme dans le cours V)
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
