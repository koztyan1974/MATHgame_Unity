using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds a playable prototype automatically in an empty scene:
/// managers, player, camera, maze visuals, NPCs, and UI.
/// </summary>
public class RuntimeGameBootstrap : MonoBehaviour
{
    private const int GridSize = 19;
    private const float CellSize = 1f;
    private const float CameraOrthoSize = 5.4f;
    private static readonly int[] VerticalWallColumns = { 4, 8, 12, 15 };
    private static readonly int[] MainOpenRows = { 2, 9, 16 };

    private static bool bootstrapped;

    private Transform worldRoot;
    private PlayerInteraction playerInteraction;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateBootstrap()
    {
        if (bootstrapped)
        {
            return;
        }

        RuntimeGameBootstrap existing = FindObjectOfType<RuntimeGameBootstrap>();
        if (existing != null)
        {
            bootstrapped = true;
            return;
        }

        GameObject go = new GameObject("RuntimeGameBootstrap");
        go.AddComponent<RuntimeGameBootstrap>();
        DontDestroyOnLoad(go);
        bootstrapped = true;
    }

    private void Start()
    {
        EnsureCoreManagers();
        EnsureRuntimeUi();
        BuildLevel(GameManager.Instance.CurrentLevelNumber);
        GameManager.Instance.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
        }
    }

    private void HandleLevelChanged(int levelNumber)
    {
        BuildLevel(levelNumber);
    }

    private void EnsureCoreManagers()
    {
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        if (QuestionManager.Instance == null)
        {
            new GameObject("QuestionManager").AddComponent<QuestionManager>();
        }

        if (UIManager.Instance == null)
        {
            new GameObject("UIManager").AddComponent<UIManager>();
        }

        if (FindObjectOfType<LevelProgressController>() == null)
        {
            new GameObject("LevelProgressController").AddComponent<LevelProgressController>();
        }
    }

    private void EnsureRuntimeUi()
    {
        UIManager ui = UIManager.Instance;
        if (ui == null)
        {
            return;
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        TMP_Text interactionText = CreateText("InteractionHint", canvas.transform, new Vector2(0f, 30f), 28, TextAlignmentOptions.Center);
        TMP_Text feedbackText = CreateText("Feedback", canvas.transform, new Vector2(0f, -10f), 28, TextAlignmentOptions.Center);
        TMP_Text levelText = CreateText("LevelLabel", canvas.transform, new Vector2(0f, 70f), 30, TextAlignmentOptions.Center);
        TMP_Text scoreText = CreateText("ScoreLabel", canvas.transform, new Vector2(-430f, 30f), 28, TextAlignmentOptions.Left);
        TMP_Text taskText = CreateText("TaskLabel", canvas.transform, new Vector2(0f, 110f), 24, TextAlignmentOptions.Center);
        TMP_Text progressText = CreateText("ProgressLabel", canvas.transform, new Vector2(420f, 30f), 24, TextAlignmentOptions.Right);
        TMP_Text npcTaskText = CreateText("NpcTaskLabel", canvas.transform, new Vector2(0f, 150f), 23, TextAlignmentOptions.Center);
        TMP_Text npcQuestListText = CreateText("NpcQuestListLabel", canvas.transform, new Vector2(-520f, 170f), 20, TextAlignmentOptions.TopLeft);
        levelText.text = "MATHgame";
        scoreText.text = "Очки: 0";
        taskText.text = "Задание уровня";
        progressText.text = "Прогресс NPC: 0/0";
        npcTaskText.text = "Задание NPC: не выбрано";
        npcQuestListText.text = "NPC задания:";

        RectTransform npcQuestRect = npcQuestListText.rectTransform;
        npcQuestRect.anchorMin = new Vector2(0f, 1f);
        npcQuestRect.anchorMax = new Vector2(0f, 1f);
        npcQuestRect.pivot = new Vector2(0f, 1f);
        npcQuestRect.sizeDelta = new Vector2(460f, 220f);
        npcQuestRect.anchoredPosition = new Vector2(20f, -20f);

        GameObject panel = CreatePanel(canvas.transform);
        TMP_Text questionText = CreateText("QuestionText", panel.transform, new Vector2(0f, 90f), 28, TextAlignmentOptions.Center);
        TMP_InputField inputField = CreateInput(panel.transform);
        Button button = CreateButton(panel.transform, "Проверить", new Vector2(0.28f, 0.08f), new Vector2(0.48f, 0.24f), new Color(0.15f, 0.55f, 0.2f));
        Button closeButton = CreateButton(panel.transform, "Закрыть", new Vector2(0.52f, 0.08f), new Vector2(0.72f, 0.24f), new Color(0.62f, 0.2f, 0.2f));

        RectTransform questionRect = questionText.rectTransform;
        questionRect.anchorMin = new Vector2(0.1f, 0.65f);
        questionRect.anchorMax = new Vector2(0.9f, 0.95f);
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.3f);
        panelRect.anchorMax = new Vector2(0.5f, 0.3f);
        panelRect.sizeDelta = new Vector2(800f, 320f);
        panelRect.anchoredPosition = Vector2.zero;

        ui.Configure(panel, questionText, inputField, button, closeButton, interactionText, feedbackText, scoreText, taskText, progressText, npcTaskText, npcQuestListText);
        ui.HideQuestionWindow();
        ui.ShowFeedback("Подойди к NPC и нажми E");
    }

    private void BuildLevel(int levelNumber)
    {
        if (worldRoot != null)
        {
            Destroy(worldRoot.gameObject);
        }

        worldRoot = new GameObject("RuntimeWorld").transform;
        BuildCamera();
        BuildMaze(levelNumber);
        BuildPlayer();
        MathNPC[] npcs = BuildNpcs(levelNumber);

        LevelProgressController progress = FindObjectOfType<LevelProgressController>();
        if (progress != null)
        {
            progress.ConfigureLevel(npcs);
        }

        UIManager.Instance?.ShowFeedback("Уровень " + levelNumber + ": реши 3 подряд у каждого NPC");
        UIManager.Instance?.SetInteractionHint(string.Empty);
        UIManager.Instance?.SetScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
        if (GameManager.Instance != null)
        {
            UIManager.Instance?.SetLevelTask(GameManager.Instance.GetCurrentLevelTaskText() + ". Цель: пройти всех 3 NPC.");
        }
        UIManager.Instance?.SetLevelProgress(0, npcs.Length);
        UIManager.Instance?.SetNpcTask("Задание NPC: подойди к любому NPC");
    }

    private void BuildCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraGo = new GameObject("Main Camera");
            camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = CameraOrthoSize;
        camera.transform.position = new Vector3(GridSize * 0.5f, GridSize * 0.5f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.11f, 0.14f, 0.22f);

        CameraFollow2D follow = camera.GetComponent<CameraFollow2D>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow2D>();
        }

        // Keep camera inside level bounds; with reduced orthographic size this avoids empty side bands.
        follow.SetBounds(new Rect(0f, 0f, GridSize, GridSize));
    }

    private void BuildMaze(int levelNumber)
    {
        Sprite tileSprite = SpriteFactory.GetSquareSprite();

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                bool borderWall = x == 0 || y == 0 || x == GridSize - 1 || y == GridSize - 1;
                bool internalWall = IsInternalWall(x, y, levelNumber);

                GameObject tile = new GameObject(borderWall || internalWall ? "Wall" : "Floor");
                tile.transform.SetParent(worldRoot);
                tile.transform.position = new Vector3(x * CellSize, y * CellSize, 0f);

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = tileSprite;
                sr.color = borderWall || internalWall ? new Color(0.12f, 0.14f, 0.2f) : new Color(0.88f, 0.9f, 0.95f);
                sr.sortingOrder = -2;

                if (borderWall || internalWall)
                {
                    BoxCollider2D wallCollider = tile.AddComponent<BoxCollider2D>();
                    wallCollider.size = Vector2.one;
                }
            }
        }
    }

    private void BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.SetParent(worldRoot);
        player.transform.position = new Vector3(1.5f, 1.5f, 0f);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.GetPlayerSprite();
        sr.color = Color.white;
        sr.sortingOrder = 5;

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.8f, 0.8f);
        player.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

        player.AddComponent<PlayerController>();
        playerInteraction = player.AddComponent<PlayerInteraction>();

        CameraFollow2D follow = Camera.main != null ? Camera.main.GetComponent<CameraFollow2D>() : null;
        if (follow != null)
        {
            follow.SetTarget(player.transform);
        }
    }

    private MathNPC[] BuildNpcs(int levelNumber)
    {
        Vector2[] spawnPoints =
        {
            new Vector2(GridSize - 2.5f, 1.5f),
            new Vector2(GridSize - 2.5f, GridSize * 0.5f),
            new Vector2(GridSize - 2.5f, GridSize - 2.5f)
        };

        MathNPC[] npcs = new MathNPC[spawnPoints.Length];
        string[] npcNames = { "Дробовик", "Алгебра", "Геометр" };
        string[] npcTasks = GetNpcTasksForLevel(levelNumber);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject npc = new GameObject("MathNPC_" + (i + 1));
            npc.transform.SetParent(worldRoot);
            npc.transform.position = new Vector3(spawnPoints[i].x, spawnPoints[i].y, 0f);

            SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.GetNpcSprite();
            sr.color = Color.white;
            sr.sortingOrder = 4;
            npc.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            CircleCollider2D trigger = npc.AddComponent<CircleCollider2D>();
            trigger.radius = 1.15f;
            trigger.isTrigger = true;

            MathNPC mathNpc = npc.AddComponent<MathNPC>();
            string npcName = i < npcNames.Length ? npcNames[i] : "Наставник " + (i + 1);
            string npcTask = i < npcTasks.Length ? npcTasks[i] : "Реши 3 примера подряд";
            mathNpc.SetupRuntimeData("NPC_" + (i + 1), npcName, npcTask);
            npcs[i] = mathNpc;
        }

        return npcs;
    }

    private string[] GetNpcTasksForLevel(int levelNumber)
    {
        switch (levelNumber)
        {
            case 1:
                return new[] { "Сложи/вычти дроби и введи ответ десятичной дробью", "Простые действия", "Устный счет" };
            case 2:
                return new[] { "Отрицательные числа", "Пропорции", "Проценты" };
            case 3:
                return new[] { "Уравнения x + b = c", "Уравнения ax = b", "Уравнения ax + b = c" };
            case 4:
                return new[] { "Квадратные корни", "Степени", "Смешанные выражения" };
            case 5:
                return new[] { "Системы уравнений", "Системы и подстановка", "Базовая тригонометрия" };
            default:
                return new[] { "Реши 3 задания подряд", "Реши 3 задания подряд", "Реши 3 задания подряд" };
        }
    }

    private bool IsInternalWall(int x, int y, int levelNumber)
    {
        if (IsProtectedCell(x, y))
        {
            return false;
        }

        // Base vertical walls with guaranteed permanent passages.
        if (IsVerticalWall(x, y))
        {
            return true;
        }

        // Additional horizontal walls from level 3 to level 5 for difficulty growth.
        if (levelNumber >= 3 && IsHorizontalWall(y, x, 6))
        {
            return true;
        }

        if (levelNumber >= 4 && IsHorizontalWall(y, x, 12))
        {
            return true;
        }

        if (levelNumber >= 5 && IsHorizontalWall(y, x, 14))
        {
            return true;
        }

        return false;
    }

    private bool IsVerticalWall(int x, int y)
    {
        if (y <= 1 || y >= GridSize - 2)
        {
            return false;
        }

        bool isWallColumn = false;
        for (int i = 0; i < VerticalWallColumns.Length; i++)
        {
            if (x == VerticalWallColumns[i])
            {
                isWallColumn = true;
                break;
            }
        }

        if (!isWallColumn)
        {
            return false;
        }

        for (int i = 0; i < MainOpenRows.Length; i++)
        {
            if (Mathf.Abs(y - MainOpenRows[i]) <= 1)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsHorizontalWall(int y, int x, int row)
    {
        if (y != row || x <= 1 || x >= GridSize - 2)
        {
            return false;
        }

        // Keep wide gates on left, center and right to preserve connectivity.
        if (Mathf.Abs(x - 2) <= 1 || Mathf.Abs(x - 9) <= 1 || Mathf.Abs(x - 16) <= 1)
        {
            return false;
        }

        return !IsProtectedCell(x, y);
    }

    private bool IsProtectedCell(int x, int y)
    {
        // Player spawn safe zone
        if (x <= 2 && y <= 2)
        {
            return true;
        }

        // NPC safe zones (right side)
        if (x >= GridSize - 3 && (Mathf.Abs(y - 2) <= 1 || Mathf.Abs(y - 9) <= 1 || Mathf.Abs(y - 16) <= 1))
        {
            return true;
        }

        return false;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("QuestionPanel");
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.75f);
        panel.AddComponent<CanvasRenderer>();
        panel.AddComponent<RectMask2D>();
        return panel;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 anchoredPosition, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = string.Empty;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(1000f, 60f);
        rect.anchoredPosition = anchoredPosition;
        return text;
    }

    private static TMP_InputField CreateInput(Transform parent)
    {
        GameObject inputRoot = new GameObject("AnswerInput");
        inputRoot.transform.SetParent(parent, false);
        Image image = inputRoot.AddComponent<Image>();
        image.color = Color.white;

        RectTransform inputRect = inputRoot.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.2f, 0.3f);
        inputRect.anchorMax = new Vector2(0.8f, 0.5f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(inputRoot.transform, false);
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 28;
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0.03f, 0f);
        textRect.anchorMax = new Vector2(0.97f, 1f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_InputField inputField = inputRoot.AddComponent<TMP_InputField>();
        inputField.textViewport = inputRect;
        inputField.textComponent = text;
        inputField.targetGraphic = image;

        return inputField;
    }

    private static Button CreateButton(
        Transform parent,
        string title,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color backgroundColor)
    {
        GameObject buttonRoot = new GameObject("CheckButton");
        buttonRoot.transform.SetParent(parent, false);
        Image image = buttonRoot.AddComponent<Image>();
        image.color = backgroundColor;

        RectTransform rect = buttonRoot.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Button button = buttonRoot.AddComponent<Button>();

        GameObject label = new GameObject("Label");
        label.transform.SetParent(buttonRoot.transform, false);
        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        text.text = title;
        text.fontSize = 30;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }
}
