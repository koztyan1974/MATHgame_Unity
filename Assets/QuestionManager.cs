using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages NPC question sessions:
/// - picks questions from current level database
/// - validates answers
/// - requires 3 correct answers in a row
/// </summary>
public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }
    private const string DecimalNpcId = "NPC_1";

    private MathNPC activeNpc;
    private QuestionSO currentQuestion;
    private int correctStreak;
    private readonly HashSet<QuestionSO> usedQuestions = new HashSet<QuestionSO>();
    private List<QuestionSO> decimalNpcQuestions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartNpcSession(MathNPC npc)
    {
        activeNpc = npc;
        correctStreak = 0;
        usedQuestions.Clear();
        if (activeNpc != null)
        {
            UIManager.Instance?.SetNpcTask("Задание NPC: " + activeNpc.NpcDisplayName + " — " + activeNpc.NpcTaskDescription + " (3 подряд)");
        }

        ShowNextQuestion();
    }

    public void SubmitAnswer(string playerAnswer)
    {
        if (activeNpc == null || currentQuestion == null)
        {
            return;
        }

        bool requiresDecimalInput = activeNpc.NpcId == DecimalNpcId;
        bool isCorrect = IsAnswerCorrect(playerAnswer, currentQuestion.correctAnswer, requiresDecimalInput);

        if (isCorrect)
        {
            correctStreak++;
            GameManager.Instance?.AddScore(1);
            UIManager.Instance?.ShowFeedback("Верно! Серия: " + correctStreak + "/3");

            if (correctStreak >= 3)
            {
                activeNpc.MarkCompleted();
                UIManager.Instance?.ShowFeedback("NPC пройден!");
                UIManager.Instance?.HideQuestionWindow();
                UIManager.Instance?.SetNpcTask("Задание NPC выполнено!");

                activeNpc = null;
                currentQuestion = null;
                return;
            }
        }
        else
        {
            if (requiresDecimalInput && !TryParseDecimal(playerAnswer, out _))
            {
                UIManager.Instance?.ShowFeedback("Введи ответ десятичной дробью: пример 0.5 или 1.25");
                return;
            }

            correctStreak = 0;
            UIManager.Instance?.ShowFeedback("Неверно. Серия сброшена (0/3).");
        }

        ShowNextQuestion();
    }

    private void ShowNextQuestion()
    {
        List<QuestionSO> sourceQuestions = GetSourceQuestionsForActiveNpc();
        if (sourceQuestions != null)
        {
            ShowQuestionFromList(sourceQuestions);
            return;
        }

        if (GameManager.Instance == null)
        {
            UIManager.Instance?.ShowFeedback("Ошибка: GameManager не найден.");
            UIManager.Instance?.HideQuestionWindow();
            return;
        }

        GameManager.Instance.EnsureDatabasesConfigured();
        LevelQuestionDatabaseSO db = GameManager.Instance.GetCurrentLevelDatabase();
        if (db == null || db.questions.Count == 0)
        {
            GameManager.Instance.AssignRuntimeDatabases(QuestionDatabaseFactory.CreateDefaultDatabases());
            db = GameManager.Instance.GetCurrentLevelDatabase();

            if (db == null || db.questions.Count == 0)
            {
                UIManager.Instance?.ShowFeedback("Вопросы для уровня не настроены.");
                UIManager.Instance?.HideQuestionWindow();
                return;
            }
        }

        ShowQuestionFromList(db.questions);
    }

    private string NormalizeAnswer(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string normalized = value.Trim().Replace(',', '.');
        return normalized;
    }

    private List<QuestionSO> GetSourceQuestionsForActiveNpc()
    {
        if (activeNpc == null)
        {
            return null;
        }

        if (activeNpc.NpcId == DecimalNpcId)
        {
            if (decimalNpcQuestions == null)
            {
                decimalNpcQuestions = BuildDecimalNpcQuestions();
            }

            return decimalNpcQuestions;
        }

        return null;
    }

    private void ShowQuestionFromList(List<QuestionSO> questions)
    {
        if (questions == null || questions.Count == 0)
        {
            UIManager.Instance?.ShowFeedback("Вопросы для NPC не настроены.");
            UIManager.Instance?.HideQuestionWindow();
            return;
        }

        List<QuestionSO> available = new List<QuestionSO>();
        for (int i = 0; i < questions.Count; i++)
        {
            if (!usedQuestions.Contains(questions[i]))
            {
                available.Add(questions[i]);
            }
        }

        if (available.Count == 0)
        {
            usedQuestions.Clear();
            available.AddRange(questions);
        }

        currentQuestion = available[Random.Range(0, available.Count)];
        usedQuestions.Add(currentQuestion);
        UIManager.Instance?.ShowQuestionWindow(currentQuestion.questionText);
    }

    private bool IsAnswerCorrect(string playerAnswer, string expectedAnswer, bool requiresDecimalInput)
    {
        if (requiresDecimalInput)
        {
            if (!TryParseDecimal(playerAnswer, out float playerValue))
            {
                return false;
            }

            if (!TryParseDecimal(expectedAnswer, out float expectedValue))
            {
                return false;
            }

            return Mathf.Abs(playerValue - expectedValue) < 0.0001f;
        }

        return string.Equals(
            NormalizeAnswer(playerAnswer),
            NormalizeAnswer(expectedAnswer),
            System.StringComparison.OrdinalIgnoreCase);
    }

    private bool TryParseDecimal(string value, out float result)
    {
        string normalized = NormalizeAnswer(value);
        return float.TryParse(normalized, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
    }

    private List<QuestionSO> BuildDecimalNpcQuestions()
    {
        List<QuestionSO> questions = new List<QuestionSO>();

        // NPC_1 special quest: fraction operations, answer in decimal form.
        AddDecimalQuestion(questions, "1) 3/8 + 1/12 = ? (введи десятичную дробь)", "0.4583");
        AddDecimalQuestion(questions, "2) 3 11/18 + 1 1/12 = ? (введи десятичную дробь)", "4.6944");
        AddDecimalQuestion(questions, "3) 5 5/6 - 2 3/4 = ? (введи десятичную дробь)", "3.0833");
        AddDecimalQuestion(questions, "4) 1/3 - 1/7 = ? (введи десятичную дробь)", "0.1905");
        AddDecimalQuestion(questions, "5) 2 1/15 - 8/45 = ? (введи десятичную дробь)", "1.8889");
        AddDecimalQuestion(questions, "6) 7/18 + 1/2 = ? (введи десятичную дробь)", "0.8889");

        return questions;
    }

    private void AddDecimalQuestion(List<QuestionSO> list, string text, string answer)
    {
        QuestionSO q = ScriptableObject.CreateInstance<QuestionSO>();
        q.questionText = text;
        q.correctAnswer = answer;
        list.Add(q);
    }
}
