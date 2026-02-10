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

    [Header("FPS Settings")]
    public bool isFirstPerson = false;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Freeze all physics rotation
        rb.useGravity = true;

        if (holdPoint == null)
        {
            GameObject hp = new GameObject("HoldPoint");
            hp.transform.SetParent(transform);
            hp.transform.localPosition = new Vector3(0.5f, -0.4f, 0.8f); // Side hand position
            holdPoint = hp.transform;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnInteract()
    {
        Debug.Log("Interact Pressed");
        if (heldItem != null) TryInteract();
        else TryPickUp();
    }

    private void Update()
    {
        // Fallback or Input System polling
        Vector2 input = Vector2.zero;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y = 1;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y = -1;
            
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x = -1;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x = 1;
            
            // Interaction
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame) OnInteract();
        }
        moveInput = input;

        // FPS Camera Look
        if (isFirstPerson && cameraTransform != null)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * 0.1f;
            float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * 0.1f;

            // Rotate Player Horizontal
            transform.Rotate(Vector3.up * mouseX);

            // Rotate Camera Vertical
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    private void FixedUpdate()
    {
        if (isFirstPerson)
        {
            // FPS Movement (Relative to facing)
            Vector3 forward = transform.forward * moveInput.y;
            Vector3 right = transform.right * moveInput.x;
            Vector3 moveDir = (forward + right).normalized;
            
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Top Down Movement
            Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            if (moveDir.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
                rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
            }
        }
        
        // Reset velocity to prevent sliding
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
    }

    private void TryPickUp()
    {
        Ray ray = GetInteractionRay();
        RaycastHit hit;
        
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red, 1f);

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            Debug.Log("Hit PickUp: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out IInteractable inter)) inter.Interact(this);
        }
    }

    private void TryInteract()
    {
        Ray ray = GetInteractionRay();
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
             Debug.Log("Hit Interact: " + hit.collider.name);
             if (hit.collider.TryGetComponent(out IInteractable inter)) inter.Interact(this);
        }
    }

    private Ray GetInteractionRay()
    {
        if (isFirstPerson && cameraTransform != null)
        {
            return new Ray(cameraTransform.position, cameraTransform.forward);
        }
        else
        {
            // Top Down Ray adjustment
            return new Ray(transform.position - Vector3.up * 0.4f, transform.forward);
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
