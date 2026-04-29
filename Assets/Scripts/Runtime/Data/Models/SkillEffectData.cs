[System.Serializable]
public struct SkillEffectData
{
    public int CoinIndex;           // 코인 번호 (0은 스킬 전체)
    public TargetType Target;       // 코인 타겟
    public EffectTiming Timing;     // 발동 타이밍
    public ConditionData Condition; // 발동 조건

    public EffectType Type;         // 실행할 행동
    public BuffType TargetBuff;     // 효과를 적용할 버프 대상
    public int Value;               // 부여 수치
}
