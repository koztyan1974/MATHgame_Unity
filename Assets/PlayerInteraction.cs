using UnityEngine;

/// <summary>
/// Handles player interactions with objects through IInteractable.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentInteractable;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        UIManager.Instance?.SetInteractionHint(interactable?.GetInteractionHint() ?? string.Empty);
    }

    public void ClearCurrentInteractable(IInteractable interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
            UIManager.Instance?.SetInteractionHint(string.Empty);
        }
    }
}
