using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public int StageID;
    public List<EnemySpawnInfo> EnemyList = new List<EnemySpawnInfo>();
}

[System.Serializable]
public class EnemySpawnInfo
{
    public int EnemyID;
    public int FormationPos;
}
