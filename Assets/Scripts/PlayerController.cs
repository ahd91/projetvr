using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera")] [SerializeField] private Camera cam;
    [Header("Movement")]
    [SerializeField] private float camSensitivity;
    [SerializeField] private float moveSensitivity;
    [SerializeField] private float gravity = -9.81f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference zqsd;
    [SerializeField] private InputActionReference mouseMovement;
    [SerializeField] private InputActionReference fire;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference slow;
    [SerializeField] private InputActionReference interact;
    
    [Header("GroundCheck")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundCheckMask;
    [Header("Weapon")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float slowDuration = 3f; // secondes de ralentissement

    [Header("Canon (clic droit la nuit)")]
    [SerializeField] private int cannonDamage = 5;       // assez pour tuer n'importe quelle créature d'un coup
    [SerializeField] private float cannonCooldown = 5f;  // secondes de recharge
    [Tooltip("Rayon de l'explosion autour du point d'impact")]
    [SerializeField] private float cannonRadius = 4f;
    [Tooltip("Pour savoir s'il fait jour ou nuit (glisser depuis la Hierarchy)")]
    [SerializeField] private DayNightManager dayNightManager;
    [Tooltip("Pour le défi « tue X créatures au canon » (glisser depuis la Hierarchy)")]
    [SerializeField] private ChallengeManager challengeManager;

    [Header("Cadeaux")]
    [Tooltip("Le sapin de la scène (glisser depuis la Hierarchy)")]
    [SerializeField] private ChristmasTree tree;
    [Tooltip("Distance maximale pour attraper un cadeau")]
    [SerializeField] private float pickRange = 3f;
    [Tooltip("Modèle de cadeau tenu devant la caméra (enfant de la Camera, SANS collider)")]
    [SerializeField] private GameObject carriedGiftVisual;

    private CharacterController controller;
    private float rotationX = 0.0f;
    private bool isGrounded = false;
    private Vector3 velocity = Vector3.zero;
    private int carriedValue = 0; // valeur du cadeau porté (0 = mains vides)
    private float cannonReadyTime = 0f; // moment où le canon sera rechargé
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (zqsd)
        {
            zqsd.action.Enable();
        }

        if (mouseMovement)
        {
            mouseMovement.action.Enable();
        }

        if (jump)
        {
            jump.action.Enable();
        }

        if (fire)
        {
            fire.action.performed += FirePressed;
            fire.action.Enable();
        }

        if (slow)
        {
            slow.action.performed += SpecialPressed;
            slow.action.Enable();
        }

        if (interact)
        {
            interact.action.performed += InteractPressed;
            interact.action.Enable();
        }

        if (carriedGiftVisual != null)
        {
            carriedGiftVisual.SetActive(false);
        }

        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Sans ça, l'abonnement survit entre deux Play (domain reload désactivé) => tir en double
    void OnDestroy()
    {
        if (fire)
        {
            fire.action.performed -= FirePressed;
        }

        if (slow)
        {
            slow.action.performed -= SpecialPressed;
        }

        if (interact)
        {
            interact.action.performed -= InteractPressed;
        }
    }

    // Rayon depuis le centre de l'écran (le viseur). Utilisé par le tir ET par la capacité.
    // Renvoie true si on a touché quelque chose, et l'objet touché dans "hit" (comme l'exemple "out" du cours)
    private bool ShootRay(out RaycastHit hit)
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
        return Physics.Raycast(ray, out hit);
    }

    // Clic gauche : enlève des PV à l'objet touché (s'il a un script Health)
    private void FirePressed(InputAction.CallbackContext obj)
    {
        // Jeu en pause (Game Over) : on ne tire plus
        if (Time.timeScale == 0)
        {
            return;
        }

        RaycastHit hit;
        if (ShootRay(out hit))
        {
            Debug.Log("Touché : " + hit.collider.name);
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    // Clic droit (capacité spéciale) : ralentisseur le jour, canon la nuit
    private void SpecialPressed(InputAction.CallbackContext obj)
    {
        // Jeu en pause (Game Over) : plus de capacité non plus
        if (Time.timeScale == 0)
        {
            return;
        }

        if (dayNightManager.IsNight())
        {
            FireCannon();
        }
        else
        {
            SlowTarget();
        }
    }

    // Le jour : ralentit la créature ou le cadeau visé
    private void SlowTarget()
    {
        RaycastHit hit;
        if (ShootRay(out hit))
        {
            Creature creature = hit.collider.GetComponent<Creature>();
            if (creature != null)
            {
                creature.Slow(slowDuration);
            }

            Gift gift = hit.collider.GetComponent<Gift>();
            if (gift != null)
            {
                gift.Slow(slowDuration);
            }
        }
    }

    // La nuit : un tir très puissant, mais il faut attendre la recharge
    private void FireCannon()
    {
        if (Time.time < cannonReadyTime)
        {
            Debug.Log("Canon en recharge...");
            return;
        }
        cannonReadyTime = Time.time + cannonCooldown;
        Debug.Log("BOUM !");

        // 1) Le rayon donne le point d'impact (si on vise le ciel, il ne se passe rien)
        RaycastHit hit;
        if (ShootRay(out hit))
        {
            // 2) Explosion : on récupère tous les colliders dans une sphère autour de l'impact
            Collider[] touched = Physics.OverlapSphere(hit.point, cannonRadius);
            foreach (Collider col in touched)
            {
                // Le joueur ne se blesse pas avec sa propre explosion
                if (col.CompareTag("Player"))
                {
                    continue;
                }

                // On ne compte que les créatures encore vivantes AVANT l'explosion et mortes APRÈS
                Health health = col.GetComponent<Health>();
                if (health != null && health.GetHp() > 0)
                {
                    health.TakeDamage(cannonDamage);
                    if (health.GetHp() <= 0)
                    {
                        challengeManager.CannonKill();
                    }
                }
            }
        }
    }

    // Temps restant avant que le canon soit prêt (0 ou moins = prêt). Utilisée par le HUD.
    public float GetCannonTimeLeft()
    {
        return cannonReadyTime - Time.time;
    }

    // Touche E : interagir avec le sapin
    private void InteractPressed(InputAction.CallbackContext obj)
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        // 1) On porte déjà un cadeau : on le dépose si on est au pied du sapin
        if (carriedValue > 0)
        {
            if (tree.IsPlayerInZone())
            {
                tree.AddGift(carriedValue);
                carriedValue = 0;
                carriedGiftVisual.SetActive(false);
            }
            else
            {
                Debug.Log("Va au pied du sapin pour déposer le cadeau");
            }
            return;
        }

        // 2) Mains vides : on essaie d'attraper le cadeau visé, s'il est assez proche
        RaycastHit hit;
        if (ShootRay(out hit) && hit.distance <= pickRange)
        {
            Gift gift = hit.collider.GetComponent<Gift>();
            if (gift != null)
            {
                if (gift.IsSlowed())
                {
                    carriedValue = gift.GetValue();
                    carriedGiftVisual.SetActive(true);
                    Destroy(gift.gameObject);
                }
                else
                {
                    Debug.Log("Il est trop rapide : ralentis-le d'abord (clic droit) !");
                }
            }
        }
    }

    // Utilisée par le HUD
    public bool IsCarryingGift()
    {
        return carriedValue > 0;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundCheckMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        float mouseX = mouseMovement.action.ReadValue<Vector2>().x * camSensitivity * Time.deltaTime;
        float mouseY = mouseMovement.action.ReadValue<Vector2>().y * camSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
        
        Vector2 zqsdValue = zqsd.action.ReadValue<Vector2>();
        controller.Move(transform.TransformDirection(new Vector3(zqsdValue.x, 0, zqsdValue.y)).normalized * moveSensitivity * Time.deltaTime);
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}