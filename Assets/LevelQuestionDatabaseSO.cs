using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Question database for one difficulty level.
/// </summary>
[CreateAssetMenu(fileName = "LevelQuestionDatabase_", menuName = "MATHgame/Level Question Database")]
public class LevelQuestionDatabaseSO : ScriptableObject
{
    [Range(1, 5)] public int levelNumber;
    public List<QuestionSO> questions = new List<QuestionSO>();
}
