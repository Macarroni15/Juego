using UnityEngine;

public class TrashStation : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        GameObject held = player.GetHeldItem();
        if (held != null)
        {
            player.ClearHeldItem();
            Destroy(held);
        }
    }
}
