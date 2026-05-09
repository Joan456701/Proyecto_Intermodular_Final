using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingTable : MonoBehaviour, IInteractable
{
    [SerializeField] private List<CraftingRecipeData> _availableRecipes;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction OnCraftingDisplayRequested;

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnCraftingDisplayRequested?.Invoke();
        interactSuccessful = true;
    }

    public void EndInteraction() { }
}
