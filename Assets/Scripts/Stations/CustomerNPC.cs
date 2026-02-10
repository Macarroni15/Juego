using UnityEngine;

public class CustomerNPC : MonoBehaviour, IInteractable
{
    // private bool hasInteracted = false;

    private void Start()
    {
        // Añadir un Collider tipo Trigger para detectar cercanía si no tiene uno
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2.0f; // Radio de detección de cercanía
    }

    // Opción 1: Interacción explícita (Tecla E/Espacio)
    public void Interact(PlayerController player)
    {
        TalkToCustomer();
    }

    // Opción 2: Interacción por cercanía (lo que pidió el usuario "cuando se acerque")
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            TalkToCustomer();
        }
    }

    private float lastInteractionTime;
    private const float COOLDOWN = 1.0f;

    private void TalkToCustomer()
    {
        if (Time.time - lastInteractionTime < COOLDOWN) return;
        lastInteractionTime = Time.time;

        Debug.Log("Cliente contactado.");
        if (KitchenBootstrap.Instance != null)
        {
            KitchenBootstrap.Instance.ShowRound();
        }
    }
}
