using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    [Header("Interaction Settings")]
    public Transform holdPoint;
    public float interactDistance = 2f;
    public LayerMask interactLayer = ~0;

    private Vector2 moveInput;
    private Rigidbody rb;
    private GameObject heldItem;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0, 0.5f, 0.8f);
            holdPoint = hp.transform;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log("Move Input: " + moveInput);
    }

    public void OnInteract()
    {
        Debug.Log("Interact Pressed");
        if (heldItem != null) TryInteract();
        else TryPickUp();
    }

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float lookSensitivity = 0.1f; // Reduced from 0.5f

    private float cameraPitch = 0f;
    private Vector2 lookInput;
    private float mouseSensitivityMultiplier = 0.5f; // Extra reducer for raw mouse input

    private void Start()
    {
        if (playerCamera != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        // Debug.Log("Look Input: " + lookInput);
    }

    private void Update()
    {
        // Cursor Unlock
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // First Person Look Rotation
        if (playerCamera != null)
        {
            Vector2 look = lookInput;
            float currentSensitivity = lookSensitivity;

            // Fallback if Event System isn't sending look events
            if (look == Vector2.zero && Mouse.current != null)
            {
                look = Mouse.current.delta.ReadValue();
                currentSensitivity *= mouseSensitivityMultiplier; // Reduce for raw mouse pixels
            }

            // Yaw (Player Body)
            transform.Rotate(Vector3.up * look.x * currentSensitivity);

            // Pitch (Camera Head)
            cameraPitch -= look.y * currentSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = moveInput;

        // Fallback if Event System isn't sending move events
        if (input == Vector2.zero && Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) input.y += 1;
            if (Keyboard.current.sKey.isPressed) input.y -= 1;
            if (Keyboard.current.aKey.isPressed) input.x -= 1;
            if (Keyboard.current.dKey.isPressed) input.x += 1;
        }

        // First Person Movement: Direction relative to player facing
        Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
        
        if (moveDir.magnitude > 0.1f)
        {
            // Move position directly
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Reset velocity safely
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        }
    }

    private void TryPickUp()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactDistance, interactLayer))
        {
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out IInteractable inter)) inter.Interact(this);
        }
    }

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactDistance, interactLayer))
        {
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out IInteractable inter)) inter.Interact(this);
        }
    }

    public void SetHeldItem(GameObject item)
    {
        heldItem = item;
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        if (item.TryGetComponent(out Rigidbody irb)) irb.isKinematic = true;
        if (item.TryGetComponent(out Collider icol)) icol.enabled = false;
    }

    public GameObject GetHeldItem() => heldItem;
    public void ClearHeldItem() => heldItem = null;
}
