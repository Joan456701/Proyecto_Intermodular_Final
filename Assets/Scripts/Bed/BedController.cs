using UnityEngine;
using UnityEngine.Events;

public class BedController : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        DayNightSpawnManager timeManager = FindFirstObjectByType<DayNightSpawnManager>();

        if (timeManager != null)
        {
            if (timeManager.CanPlayerSleep(out string failReason))
            {
                timeManager.SkipToNextDay();
                interactSuccessful = true;
            }
            else
            {
                Debug.Log("Cama: " + failReason);
                interactSuccessful = false;
            }
        }
        else
        {
            interactSuccessful = false;
        }
    }

    public void EndInteraction() { }
}
