using System;

public enum ConditionType
{
    None,
    IfFaster,
    IfFasterBy2,
    BuffSum,
    IfHasBuff,
    IfNotHasBuff,
    IfSelfHasBuff,
}

[System.Serializable]
public struct ConditionData
{
    public ConditionType Type;

    public int TargetValue;
    public BuffType[] TargetBuffs;

    public ConditionData(ConditionType type, int targetValue = 0, BuffType[] targetBuffs = null)
    {
        Type = type;
        TargetValue = targetValue;
        TargetBuffs = targetBuffs ?? new BuffType[0];
    }
}
