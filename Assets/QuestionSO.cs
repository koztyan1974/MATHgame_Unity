using UnityEngine;

[CreateAssetMenu(fileName = "Question_", menuName = "MATHgame/Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea] public string questionText;
    public string correctAnswer;
}
