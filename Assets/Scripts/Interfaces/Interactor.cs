using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public bool isInteracting { get; private set; }
    
    public void StartInteraction(IInteractable interactuable)
    {
        interactuable.Interact(this, out bool interactSuccessful);
        isInteracting = interactSuccessful;
    }

    public void EndInteraction()
    {
        isInteracting = false;
    }
}
