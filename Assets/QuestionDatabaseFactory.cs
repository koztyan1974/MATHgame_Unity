using UnityEngine;

/// <summary>
/// Creates in-memory ScriptableObject databases as a fallback,
/// so the project works immediately after import.
/// </summary>
public static class QuestionDatabaseFactory
{
    public static LevelQuestionDatabaseSO[] CreateDefaultDatabases()
    {
        LevelQuestionDatabaseSO[] databases = new LevelQuestionDatabaseSO[5];
        for (int i = 0; i < databases.Length; i++)
        {
            databases[i] = ScriptableObject.CreateInstance<LevelQuestionDatabaseSO>();
            databases[i].levelNumber = i + 1;
        }

        // Level 1 (Grade 5): Fractions and basic arithmetic
        Add(databases[0], "1/2 + 1/4 = ?", "3/4");
        Add(databases[0], "3/5 - 1/5 = ?", "2/5");
        Add(databases[0], "2/3 + 1/3 = ?", "1");
        Add(databases[0], "4/8 = ? (simplify)", "1/2");
        Add(databases[0], "15 + 27 = ?", "42");
        Add(databases[0], "48 - 19 = ?", "29");
        Add(databases[0], "12 * 3 = ?", "36");
        Add(databases[0], "56 / 7 = ?", "8");
        Add(databases[0], "3 * (4 + 2) = ?", "18");
        Add(databases[0], "7 + 8 - 5 = ?", "10");

        // Level 2 (Grade 6): Negative numbers and proportions
        Add(databases[1], "-7 + 3 = ?", "-4");
        Add(databases[1], "5 - 12 = ?", "-7");
        Add(databases[1], "-4 * 6 = ?", "-24");
        Add(databases[1], "-3 * -5 = ?", "15");
        Add(databases[1], "18 / (-3) = ?", "-6");
        Add(databases[1], "-20 / (-4) = ?", "5");
        Add(databases[1], "4:6 = 10:x. x = ?", "15");
        Add(databases[1], "3:5 = 12:x. x = ?", "20");
        Add(databases[1], "x/8 = 3/4. x = ?", "6");
        Add(databases[1], "25% of 80 = ?", "20");

        // Level 3 (Grade 7): Linear equations x + b = c and variants
        Add(databases[2], "x + 5 = 12. x = ?", "7");
        Add(databases[2], "x - 8 = -2. x = ?", "6");
        Add(databases[2], "x + (-4) = 9. x = ?", "13");
        Add(databases[2], "x + 17 = 30. x = ?", "13");
        Add(databases[2], "x - 14 = 5. x = ?", "19");
        Add(databases[2], "2x = 18. x = ?", "9");
        Add(databases[2], "3x = 27. x = ?", "9");
        Add(databases[2], "x/4 = 6. x = ?", "24");
        Add(databases[2], "2x + 3 = 11. x = ?", "4");
        Add(databases[2], "5x - 10 = 15. x = ?", "5");

        // Level 4 (Grade 8): Roots and powers
        Add(databases[3], "sqrt(49) = ?", "7");
        Add(databases[3], "sqrt(81) = ?", "9");
        Add(databases[3], "sqrt(121) = ?", "11");
        Add(databases[3], "2^5 = ?", "32");
        Add(databases[3], "3^4 = ?", "81");
        Add(databases[3], "5^3 = ?", "125");
        Add(databases[3], "sqrt(64) + 2^3 = ?", "16");
        Add(databases[3], "sqrt(144) - 3^2 = ?", "3");
        Add(databases[3], "(2^3) + (3^2) = ?", "17");
        Add(databases[3], "sqrt(100) + sqrt(25) = ?", "15");

        // Level 5 (Grade 9): Systems of equations and basic trigonometry
        Add(databases[4], "x + y = 5, x - y = 1. x = ?", "3");
        Add(databases[4], "x + y = 5, x - y = 1. y = ?", "2");
        Add(databases[4], "x + y = 8, x - y = 2. y = ?", "3");
        Add(databases[4], "x + y = 12, x - y = 4. x = ?", "8");
        Add(databases[4], "2x + y = 11, x - y = 1. x = ?", "4");
        Add(databases[4], "2x + y = 11, x - y = 1. y = ?", "3");
        Add(databases[4], "sin(30) = ?", "0.5");
        Add(databases[4], "cos(60) = ?", "0.5");
        Add(databases[4], "sin(90) = ?", "1");
        Add(databases[4], "tan(45) = ?", "1");

        return databases;
    }

    private static void Add(LevelQuestionDatabaseSO db, string text, string answer)
    {
        QuestionSO question = ScriptableObject.CreateInstance<QuestionSO>();
        question.questionText = text;
        question.correctAnswer = answer;
        db.questions.Add(question);
    }
}
