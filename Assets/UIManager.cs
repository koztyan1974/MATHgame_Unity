using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages interaction UI:
/// - question window
/// - input field
/// - check button
/// - hints and feedback
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Question Window")]
    [SerializeField] private GameObject questionWindow;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button closeButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text interactionHintText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelTaskText;
    [SerializeField] private TMP_Text levelProgressText;
    [SerializeField] private TMP_Text npcTaskText;
    [SerializeField] private TMP_Text npcQuestListText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (checkButton != null)
        {
            checkButton.onClick.AddListener(OnCheckClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        HideQuestionWindow();
        SetInteractionHint(string.Empty);
        ShowFeedback(string.Empty);
        SetScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += SetScore;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= SetScore;
        }

        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(OnCheckClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }

    public void Configure(
        GameObject questionPanel,
        TMP_Text questionLabel,
        TMP_InputField inputField,
        Button submitButton,
        Button closeQuestionButton,
        TMP_Text interactionHintLabel,
        TMP_Text feedbackLabel,
        TMP_Text scoreLabel,
        TMP_Text taskLabel,
        TMP_Text progressLabel,
        TMP_Text npcTaskLabel,
        TMP_Text npcQuestListLabel)
    {
        questionWindow = questionPanel;
        questionText = questionLabel;
        answerInput = inputField;
        feedbackText = feedbackLabel;
        interactionHintText = interactionHintLabel;
        scoreText = scoreLabel;
        levelTaskText = taskLabel;
        levelProgressText = progressLabel;
        npcTaskText = npcTaskLabel;
        npcQuestListText = npcQuestListLabel;

        if (checkButton != null)
        {
            checkButton.onClick.RemoveListener(OnCheckClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        checkButton = submitButton;
        if (checkButton != null)
        {
            checkButton.onClick.AddListener(OnCheckClicked);
        }

        closeButton = closeQuestionButton;
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && questionWindow != null && questionWindow.activeSelf)
        {
            OnCloseClicked();
        }
    }

    public void ShowQuestionWindow(string question)
    {
        if (questionWindow == null || questionText == null || answerInput == null)
        {
            return;
        }

        questionWindow.SetActive(true);
        questionText.text = question;
        answerInput.text = string.Empty;
        answerInput.ActivateInputField();
    }

    public void HideQuestionWindow()
    {
        if (questionWindow != null)
        {
            questionWindow.SetActive(false);
        }
    }

    public void SetInteractionHint(string hint)
    {
        if (interactionHintText != null)
        {
            interactionHintText.text = hint;
        }
    }

    public void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Очки: " + score;
        }
    }

    public void SetLevelTask(string taskText)
    {
        if (levelTaskText != null)
        {
            levelTaskText.text = taskText;
        }
    }

    public void SetLevelProgress(int completedNpcCount, int totalNpcCount)
    {
        if (levelProgressText != null)
        {
            levelProgressText.text = "Прогресс NPC: " + completedNpcCount + "/" + totalNpcCount;
        }
    }

    public void SetNpcTask(string taskText)
    {
        if (npcTaskText != null)
        {
            npcTaskText.text = taskText;
        }
    }

    public void SetNpcQuestList(string questListText)
    {
        if (npcQuestListText != null)
        {
            npcQuestListText.text = questListText;
        }
    }

    private void OnCheckClicked()
    {
        if (QuestionManager.Instance != null && answerInput != null)
        {
            QuestionManager.Instance.SubmitAnswer(answerInput.text);
        }
    }

    private void OnCloseClicked()
    {
        HideQuestionWindow();
        SetNpcTask("Задание NPC: не выбрано");
        ShowFeedback("Окно вопроса закрыто.");
    }
}
