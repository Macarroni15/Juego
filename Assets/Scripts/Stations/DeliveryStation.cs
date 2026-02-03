using UnityEngine;

public class DeliveryStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        GameObject held = player.GetHeldItem();
        if (held != null)
        {
            GameManager.Instance.AddScore(100);
            player.ClearHeldItem();
            Destroy(held);
        }
    }
}
