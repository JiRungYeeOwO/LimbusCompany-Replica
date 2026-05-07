using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : BattleCharacter
{
    public override void Initialize(CharacterBaseData data)
    {
        base.Initialize(data);

        if (data is IdentityData identityData)
        {
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
}
