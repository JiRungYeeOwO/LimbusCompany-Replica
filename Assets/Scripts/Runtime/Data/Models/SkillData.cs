using System.Collections.Generic;

[System.Serializable]
public class SkillData
{
    public int SkillID;
    public string SkillName;
    public int SkillPosition;
    public string SkillType;
    public string SinAttribute;
    public int BasePower;
    public int CoinPower;
    public int AttackWeight;
    public int CoinCount;

    public List<SkillEffectData> Effects = new List<SkillEffectData>();
}
