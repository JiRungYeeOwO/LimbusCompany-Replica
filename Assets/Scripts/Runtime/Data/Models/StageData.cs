using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public int StageID;
    public string BackgroundPrefabPath;
    public string BGMName;
    public List<EnemySpawnInfo> EnemyList = new List<EnemySpawnInfo>();
}

[System.Serializable]
public class EnemySpawnInfo
{
    public int EnemyID;
    public int FormationPos;
}
