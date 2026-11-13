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
    
    [Header("GroundCheck")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundCheckMask;
    [Header("Weapon")]
    [SerializeField] private int damage = 1;

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
    }

    private void FirePressed(InputAction.CallbackContext obj)
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Touché : " + hit.collider.name);
            // InParent : le collider peut être sur un enfant du modèle 3D
            Health health = hit.collider.GetComponentInParent<Health>();
            if (health)
            {
                health.TakeDamage(damage);
            }
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