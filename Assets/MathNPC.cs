using System;
using UnityEngine;

/// <summary>
/// NPC with a trigger area for math dialog.
/// Player must answer 3 questions in a row to complete this NPC.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MathNPC : MonoBehaviour, IInteractable
{
    public static event Action<MathNPC> OnNpcCompleted;

    [SerializeField] private string npcId = "NPC_01";
    [SerializeField] private string npcDisplayName = "Наставник";
    [SerializeField] [TextArea] private string npcTaskDescription = "Реши 3 задачи подряд";
    [SerializeField] private PassageGate gateToOpen;

    private bool isCompleted;

    public string NpcId => npcId;
    public string NpcDisplayName => npcDisplayName;
    public string NpcTaskDescription => npcTaskDescription;
    public bool IsCompleted => isCompleted;

    public void Interact(PlayerInteraction interactor)
    {
        if (isCompleted)
        {
            return;
        }

        QuestionManager.Instance.StartNpcSession(this);
    }

    public string GetInteractionHint()
    {
        return isCompleted ? npcDisplayName + " уже пройден" : "Нажми E: " + npcDisplayName;
    }

    public string GetQuestListLine()
    {
        string marker = isCompleted ? "[x] " : "[ ] ";
        return marker + npcDisplayName + ": " + npcTaskDescription;
    }

    public void SetupRuntimeData(string id, string displayName, string taskDescription)
    {
        npcId = id;
        npcDisplayName = displayName;
        npcTaskDescription = taskDescription;
    }

    public void MarkCompleted()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        gateToOpen?.OpenGate();
        OnNpcCompleted?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.SetCurrentInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ClearCurrentInteractable(this);
        }
    }
}
