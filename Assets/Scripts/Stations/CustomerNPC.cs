using UnityEngine;

public class CustomerNPC : MonoBehaviour, IInteractable
{
    private void Start()
    {
        // Añadir un Collider tipo Trigger para detectar cercanía si no tiene uno
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 2.0f; // Radio de detección de cercanía
    }

    // Opción 1: Interacción explícita (Tecla E/Espacio o Click)
    public void Interact(PlayerController player)
    {
        TalkToCustomer();
    }

    // Opción 2: Mostrar Botón/Prompt cuando está cerca
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            if (KitchenBootstrap.Instance != null)
            {
                // Muestra el botón arriba a la izquierda
                KitchenBootstrap.Instance.ToggleInteractionPrompt(true, () => TalkToCustomer());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            if (KitchenBootstrap.Instance != null)
            {
                // Oculta el botón
                KitchenBootstrap.Instance.ToggleInteractionPrompt(false);
            }
        }
    }

    private float lastInteractionTime;
    private const float COOLDOWN = 1.0f;

    private void TalkToCustomer()
    {
        if (Time.time - lastInteractionTime < COOLDOWN) return;
        lastInteractionTime = Time.time;

        Debug.Log("Iniciando conversación con cliente...");
        
        if (KitchenBootstrap.Instance != null)
        {
            // Marcamos este cliente como el que está siendo atendido
            KitchenBootstrap.Instance.currentCustomerServed = this;
            KitchenBootstrap.Instance.ToggleInteractionPrompt(false);
            KitchenBootstrap.Instance.ShowRound();
        }
    }

    public void SatisfyAndLeave()
    {
        Debug.Log("Cliente satisfecho, retirando NPC...");
        // Notificar al manager para liberar el hueco
        if (KitchenBootstrap.Instance != null)
        {
            KitchenBootstrap.Instance.OnCustomerLeft(gameObject);
        }
        Destroy(gameObject);
    }
}
