using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public Transform cam;
    public float mouseSensitivity = 1.2f;
    public float maxLookX = 85f;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0;

    CharacterController cc;
    Vector3 velocity;
    float rotX;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Bail early while paused (no camera, no movement, no interaction)
        if (GamePauseController.IsPaused) return;

        Look();
        Move();
        Interact();
    }

    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * 10f * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * 10f * mouseSensitivity;

        transform.Rotate(0f, mx, 0f);
        rotX = Mathf.Clamp(rotX - my, -maxLookX, maxLookX);
        if (cam) cam.localRotation = Quaternion.Euler(rotX, 0f, 0f);
    }

    void Move()
    {
        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = (transform.right * x + transform.forward * z).normalized;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        cc.Move(input * speed * Time.deltaTime);

        if (grounded && Input.GetButtonDown("Jump"))
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }

    void Interact()
    {
        if (!cam) return;

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Collide))
        {
            var all = hit.collider.GetComponentsInParent<IInteractable>(true);
            IInteractable chosen = null;

            for (int i = 0; i < all.Length; i++)
                if (all[i] is PowerGatedInteractable) { chosen = all[i]; break; }

            if (chosen == null && all.Length > 0) chosen = all[0];

            if (chosen != null && Input.GetMouseButtonDown(0))
                chosen.Interact(transform);
        }
    }
}
