using UnityEngine;
using UnityEngine.InputSystem;

namespace CookingGame
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Interaction Settings")]
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private LayerMask interactLayer;

        private Vector2 moveInput;
        private Rigidbody rb;
        private GameObject heldItem;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnInteract()
        {
            if (heldItem != null)
            {
                // Logic to drop or place item will go here
                TryPlaceItem();
            }
            else
            {
                // Logic to pick up item
                TryPickUpItem();
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            
            if (moveDirection.magnitude >= 0.1f)
            {
                // Rotation
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

                // Movement
                rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            }
        }

        private void TryPickUpItem()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactDistance, interactLayer))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact(this);
                }
            }
        }

        public void SetHeldItem(GameObject item)
        {
            heldItem = item;
            item.transform.SetParent(holdPoint);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        public GameObject GetHeldItem() => heldItem;
        public void ClearHeldItem() => heldItem = null;
    }
}
