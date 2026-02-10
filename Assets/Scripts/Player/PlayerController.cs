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

    private void Update()
    {
        // Fallback Input (Direct Keyboard poll)
        // Useful if PlayerInput component is not set up with an Action Asset
        if (Keyboard.current != null)
        {
            Vector2 keyboardInput = Vector2.zero;
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) keyboardInput.x = -1;
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) keyboardInput.x = 1;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) keyboardInput.y = 1;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) keyboardInput.y = -1;

            // Only override if there is input, or if we suspect no other input system is driving
            if (keyboardInput != Vector2.zero) 
            {
                moveInput = keyboardInput;
            }
            else
            {
                // If keyboard is not pressed, and we haven't received an event recently...
                // Actually, let's just use keyboardInput if it's non-zero, otherwise let it be 0 (stops)
                // EXCEPT if OnMove is driving it.
                // Simple hack: Always update moveInput from keyboard if PlayerInput is missing.
                if (GetComponent<PlayerInput>() == null || GetComponent<PlayerInput>().actions == null)
                {
                    moveInput = keyboardInput;
                }
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            {
                OnInteract();
            }
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        if (moveDir.magnitude > 0.1f)
        {
            // Rotate towards direction (Instant or smooth)
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            
            // Move
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Reset velocity safely to prevent sliding
             rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        }
    }

    private void TryPickUp()
    {
        RaycastHit hit;
        // Adjusted for Top-Down view where stations are lower than player center
        // Player (Capsule) Center Y=1. Table Center Y=0.5. 
        // We use an origin closer to the table height.
        if (Physics.Raycast(transform.position - Vector3.up * 0.4f, transform.forward, out hit, interactDistance, interactLayer))
        {
            Debug.Log("Hit PickUp: " + hit.collider.name);
            if (hit.collider.TryGetComponent(out IInteractable inter)) inter.Interact(this);
        }
    }

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position - Vector3.up * 0.4f, transform.forward, out hit, interactDistance, interactLayer))
        {
             Debug.Log("Hit Interact: " + hit.collider.name);
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
