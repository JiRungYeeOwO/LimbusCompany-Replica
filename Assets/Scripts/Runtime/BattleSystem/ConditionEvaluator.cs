using System;
using System.Collections.Generic;

public static class ConditionEvaluator
{
    private static readonly Dictionary<ConditionType, Func<ConditionData, BattleCharacter, BattleCharacter, bool>> _evaluators = new()
    {
        {
            ConditionType.None,
            (condition, caster, target) => true
        },

        {
            ConditionType.IfFaster,
            (condition, caster, target) => caster.Speed > target.Speed
        },

        {
            ConditionType.IfFasterBy2,
            (condition, caster, target) => (caster.Speed - target.Speed) >= 2
        },

        {
            ConditionType.BuffSum,
            (condition, caster, target) =>
            {
                int currentSum = 0;

                if (condition.TargetBuffs != null)
                {
                    foreach (BuffType buff in condition.TargetBuffs)
                    {
                        currentSum += target.GetBuffValue(buff);
                    }
                }

                return currentSum >= condition.TargetValue;
            }
        },

        {
            ConditionType.IfHasBuff,
            (condition, caster, target) =>
            {
                if (condition.TargetBuffs != null && condition.TargetBuffs.Length > 0)
                {
                    return target.GetBuffValue(condition.TargetBuffs[0]) > 0;
                }

                return false;
            }
        }
    };

    public static bool Evaluate(ConditionData condition, BattleCharacter caster, BattleCharacter target)
    {
        if (_evaluators.TryGetValue(condition.Type, out var evalFunc))
        {
            return evalFunc(condition, caster, target);
        }

        return false;
    }
}
