using UnityEngine;

public struct SKillSelectedEvent : IEvent
{
    public PlayerCharacter Player;
    public SkillData Skill;
    public int Index;
}

public struct SkillDragStartedEvent : IEvent
{
    public SkillSlotUI Slot;
    public PlayerCharacter Player;
    public SkillData Skill;
    public int Index;
}

public struct SkillDragUpdatedEvent : IEvent
{
    public Vector2 MousePos;
}

public struct SkillDragEndedEvent : IEvent
{
    public Vector2 MousePos;
}