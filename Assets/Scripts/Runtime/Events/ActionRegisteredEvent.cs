public struct ActionRegisteredEvent : IEvent
{
    public BattleCharacter Attacker;
    public int AttackerSlotIndex;

    public BattleCharacter Target;
    public int TargetSlotIndex;

    public SkillData Skill;
}
