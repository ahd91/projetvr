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

    [Header("Cadeaux")]
    [Tooltip("Le sapin de la scène (glisser depuis la Hierarchy)")]
    [SerializeField] private ChristmasTree tree;

    private CharacterController controller;
    private float rotationX = 0.0f;
    private bool isGrounded = false;
    private Vector3 velocity = Vector3.zero;
    
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
            slow.action.performed += SlowPressed;
            slow.action.Enable();
        }

        if (interact)
        {
            interact.action.performed += InteractPressed;
            interact.action.Enable();
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
            slow.action.performed -= SlowPressed;
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

    // Clic droit (capacité spéciale) : ralentit la créature touchée
    private void SlowPressed(InputAction.CallbackContext obj)
    {
        // Jeu en pause (Game Over) : plus de capacité non plus
        if (Time.timeScale == 0)
        {
            return;
        }

        RaycastHit hit;
        if (ShootRay(out hit))
        {
            Creature creature = hit.collider.GetComponent<Creature>();
            if (creature != null)
            {
                creature.Slow(slowDuration);
            }
        }
    }

    // Touche E : interagir avec le sapin
    private void InteractPressed(InputAction.CallbackContext obj)
    {
        if (Time.timeScale == 0)
        {
            return;
        }

        // TEST ÉTAPE B : dans la zone du sapin, E ajoute un cadeau.
        // À l'étape C, on ne pourra déposer que si on porte un cadeau.
        if (tree != null && tree.IsPlayerInZone())
        {
            tree.AddGift(1);
        }
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