using UnityEngine;

/// <summary>
/// Tracks current level progress.
/// When all level NPCs are completed, loads the next level.
/// </summary>
public class LevelProgressController : MonoBehaviour
{
    [SerializeField] private MathNPC[] levelNpcs;
    [SerializeField] private float nextLevelDelay = 1.5f;

    private int completedCount;
    private bool levelCompleted;

    private void OnEnable()
    {
        MathNPC.OnNpcCompleted += HandleNpcCompleted;
    }

    private void OnDisable()
    {
        MathNPC.OnNpcCompleted -= HandleNpcCompleted;
    }

    private void Start()
    {
        RefreshNpcListIfEmpty();
        ResetProgress();
    }

    public void ConfigureLevel(MathNPC[] npcs)
    {
        levelNpcs = npcs;
        ResetProgress();
        UIManager.Instance?.SetLevelProgress(completedCount, levelNpcs != null ? levelNpcs.Length : 0);
        UpdateNpcQuestListUi();
    }

    private void HandleNpcCompleted(MathNPC npc)
    {
        if (levelNpcs == null || levelNpcs.Length == 0)
        {
            RefreshNpcListIfEmpty();
        }

        if (levelCompleted || !IsNpcFromThisLevel(npc))
        {
            return;
        }

        completedCount++;
        UIManager.Instance?.ShowFeedback("NPC пройдено: " + completedCount + "/" + levelNpcs.Length);
        UIManager.Instance?.SetLevelProgress(completedCount, levelNpcs.Length);
        UpdateNpcQuestListUi();

        if (completedCount >= levelNpcs.Length)
        {
            levelCompleted = true;
            UIManager.Instance?.ShowFeedback("Уровень пройден. Загружаем следующий...");
            Invoke(nameof(LoadNextLevel), nextLevelDelay);
        }
    }

    private bool IsNpcFromThisLevel(MathNPC npc)
    {
        for (int i = 0; i < levelNpcs.Length; i++)
        {
            if (levelNpcs[i] == npc)
            {
                return true;
            }
        }

        return false;
    }

    private void LoadNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteCurrentLevel();
        }
    }

    private void RefreshNpcListIfEmpty()
    {
        if (levelNpcs == null || levelNpcs.Length == 0)
        {
            levelNpcs = FindObjectsOfType<MathNPC>();
        }
    }

    private void ResetProgress()
    {
        completedCount = 0;
        levelCompleted = false;
        UIManager.Instance?.SetLevelProgress(0, levelNpcs != null ? levelNpcs.Length : 0);
        UpdateNpcQuestListUi();
    }

    private void UpdateNpcQuestListUi()
    {
        if (levelNpcs == null || levelNpcs.Length == 0)
        {
            UIManager.Instance?.SetNpcQuestList("NPC задания: нет данных");
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("NPC задания:");
        for (int i = 0; i < levelNpcs.Length; i++)
        {
            if (levelNpcs[i] != null)
            {
                sb.AppendLine(levelNpcs[i].GetQuestListLine());
            }
        }

        UIManager.Instance?.SetNpcQuestList(sb.ToString().TrimEnd());
    }
}
