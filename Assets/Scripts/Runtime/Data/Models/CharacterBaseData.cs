using UnityEngine;

[System.Serializable]
public abstract class CharacterBaseData
{
    public string Name;
    public int MaxHP;
    public string SpeedRange;
    public System.Collections.Generic.List<int> SkillIDs = new System.Collections.Generic.List<int>();

    public (int min, int max) GetSpeed()
    {
        var split = SpeedRange.Split('-');
        return (int.Parse(split[0]), int.Parse(split[1]));
    }
}
