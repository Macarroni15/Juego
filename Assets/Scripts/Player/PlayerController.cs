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

    private void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
            
            // Unity 6 uses linearVelocity, but we can check if it exists or use movePosition
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
