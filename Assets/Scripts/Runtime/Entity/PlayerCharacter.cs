using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BattleCharacter
{
    private IdentityData _myIdentityData;

    private List<SkillData> _fullDeck = new List<SkillData>();
    private List<SkillData> _currentHand = new List<SkillData>();

    private List<int> _skillDeck = new List<int>();
    private const int REFILL_THRESHOLD = 2;

    public override void Initialize(CharacterBaseData data)
    {
        base.Initialize(data);

        if (data is IdentityData identityData)
        {
            _myIdentityData = identityData;

            RefillDeck(identityData);

            CustomLogger.LogSystem($"[Player] 수감자 ID {identityData.SinnerID}");
        }
    }

    public override void DetermineTarget(List<BattleCharacter> enemies)
    {
        if (CurrentTarget != null) return;

        if (enemies != null && enemies.Count > 0)
        {
            CurrentTarget = enemies[0];
            CustomLogger.LogBattle($"[Player] {gameObject.name}의 자동 타겟팅: {CurrentTarget.gameObject.name}");
        }
    }

    public List<SkillData> GetSkillList()
    {
        List<SkillData> skills = new List<SkillData>();

        if (_myIdentityData == null || _myIdentityData.SkillIDs == null)
        {
            CustomLogger.Error($"[Player] {gameObject.name}의 인격 데이터가 로드되지 않았습니다.");
            return skills;
        }

        CustomLogger.LogSystem($"[Debug] {_myIdentityData.Name}의 SkillIDs 개수: {_myIdentityData.SkillIDs.Count}");

        foreach (int skillID in _myIdentityData.SkillIDs)
        {
            if (skillID == 0) continue;

            SkillData skill = DataManager.Instance.GetSkill(skillID);

            if (skill != null)
            {
                skills.Add(skill);
            }
        }

        return skills;
    }

    public List<SkillData> GetCurrentHand(int count)
    {
        if (_skillDeck.Count <= REFILL_THRESHOLD)
        {
            RefillDeck(_myIdentityData);
        }

        List<SkillData> hand = new List<SkillData>();

        for (int i = 0; i < count; i++)
        {
            if (i < _skillDeck.Count)
            {
                hand.Add(DataManager.Instance.GetSkill(_skillDeck[i]));
            }
        }

        return hand;
    }

    public void UseSkill(int index)
    {
        if (_skillDeck.Count > index)
        {
            _skillDeck.RemoveAt(index);
        }
    }

    public List<SkillData> GetSkillQueue(int count)
    {
        return GetCurrentHand(count);
    }

    private void RefillDeck(IdentityData data)
    {
        List<int> newSack = new List<int>();

        for (int i = 0; i < 3; i++) newSack.Add(data.SkillIDs[0]);
        for (int i = 0; i < 2; i++) newSack.Add(data.SkillIDs[1]);
        for (int i = 0; i < 1; i++) newSack.Add(data.SkillIDs[2]);

        for (int i = 0; i < newSack.Count; i++)
        {
            int temp = newSack[i];
            int randomIdx = Random.Range(i, newSack.Count);
            newSack[i] = newSack[randomIdx];
            newSack[randomIdx] = temp;
        }

        _skillDeck.AddRange(newSack);

        CustomLogger.LogSystem($"{gameObject.name}의 덱 보충 완료. 현재 총 {_skillDeck.Count}장.");
    }
}
