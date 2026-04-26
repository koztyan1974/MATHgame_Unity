using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Global game manager:
/// - stores current level index
/// - provides current level question database
/// - handles level transitions between 5 scenes
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public event Action<int> OnLevelChanged;
    public event Action<int> OnScoreChanged;

    [Header("Levels")]
    [SerializeField] private string[] levelSceneNames = { "Level_1", "Level_2", "Level_3", "Level_4", "Level_5" };

    [Header("Question Databases (index 0 => Level 1)")]
    [SerializeField] private LevelQuestionDatabaseSO[] levelDatabases = new LevelQuestionDatabaseSO[5];

    private int currentLevelIndex;
    private int score;

    public int CurrentLevelNumber => currentLevelIndex + 1;
    public int Score => score;

    public string GetCurrentLevelTaskText()
    {
        switch (CurrentLevelNumber)
        {
            case 1:
                return "Уровень 1: дроби и простые действия";
            case 2:
                return "Уровень 2: отрицательные числа и пропорции";
            case 3:
                return "Уровень 3: линейные уравнения";
            case 4:
                return "Уровень 4: корни и степени";
            case 5:
                return "Уровень 5: системы уравнений и тригонометрия";
            default:
                return "Решай задачи и проходи NPC";
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureDatabasesConfigured();
        ResetScore();
    }

    public LevelQuestionDatabaseSO GetCurrentLevelDatabase()
    {
        EnsureDatabasesConfigured();

        if (currentLevelIndex < 0 || currentLevelIndex >= levelDatabases.Length)
        {
            return null;
        }

        return levelDatabases[currentLevelIndex];
    }

    public void CompleteCurrentLevel()
    {
        if (currentLevelIndex >= levelSceneNames.Length - 1)
        {
            UIManager.Instance?.ShowFeedback("Поздравляем! Все 5 уровней пройдены.");
            return;
        }

        currentLevelIndex++;
        OnLevelChanged?.Invoke(CurrentLevelNumber);

        if (CanLoadConfiguredScene(currentLevelIndex))
        {
            SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
        }
    }

    public void LoadLevel(int levelNumber)
    {
        int index = Mathf.Clamp(levelNumber - 1, 0, levelSceneNames.Length - 1);
        currentLevelIndex = index;
        OnLevelChanged?.Invoke(CurrentLevelNumber);

        if (CanLoadConfiguredScene(currentLevelIndex))
        {
            SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
        }
    }

    public void AssignRuntimeDatabases(LevelQuestionDatabaseSO[] databases)
    {
        if (databases == null || databases.Length == 0)
        {
            return;
        }

        levelDatabases = databases;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }

    public void EnsureDatabasesConfigured()
    {
        bool hasData = true;
        if (levelDatabases == null || levelDatabases.Length != 5)
        {
            hasData = false;
        }
        else
        {
            for (int i = 0; i < levelDatabases.Length; i++)
            {
                if (levelDatabases[i] == null || levelDatabases[i].questions == null || levelDatabases[i].questions.Count == 0)
                {
                    hasData = false;
                    break;
                }
            }
        }

        if (!hasData)
        {
            levelDatabases = QuestionDatabaseFactory.CreateDefaultDatabases();
        }
    }

    private bool CanLoadConfiguredScene(int levelIndex)
    {
        if (levelSceneNames == null || levelIndex < 0 || levelIndex >= levelSceneNames.Length)
        {
            return false;
        }

        string sceneName = levelSceneNames[levelIndex];
        return !string.IsNullOrWhiteSpace(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);
    }
}
