using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BattleCharacter
{
    private IdentityData _myIdentityData;

    public override void Initialize(CharacterBaseData data)
    {
        base.Initialize(data);

        if (data is IdentityData identityData)
        {
            _myIdentityData = identityData;

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
}
