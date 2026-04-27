using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillParser
{
    public static List<SkillEffectData> ParseFullEffectString(string rawString)
    {
        List<SkillEffectData> effects = new List<SkillEffectData>();

        if (string.IsNullOrEmpty(rawString)) return effects;

        string[] separatedEffects = rawString.Split('|');

        foreach (string effectStr in separatedEffects)
        {
            string cleanStr = effectStr.Trim();
            if (string.IsNullOrEmpty(cleanStr)) continue;

            try
            {
                SkillEffectData parsedData = ParseSingleEffect(cleanStr);
                effects.Add(parsedData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SkillParser] 문자열 해석 실패: '{cleanStr}'\n사유: {e.Message}");
            }
        }

        return effects;
    }

    private static SkillEffectData ParseSingleEffect(string singleEffectStr)
    {
        SkillEffectData data = new SkillEffectData();
        data.Condition = new ConditionData(ConditionType.None);

        // '@' 기준으로 코인 번호와 나머지 분리
        string[] atSplit = singleEffectStr.Split('@');

        data.CoinIndex = int.Parse(atSplit[0].Trim().Trim());
        data.Target = (TargetType)Enum.Parse(typeof(TargetType), atSplit[1].Trim());

        string rest = atSplit[2].Trim();

        // ':' 기준으로 (타이밍/조건)과 (효과) 분리
        string[] colonSplit = rest.Split(':');
        string timingAndCondition = colonSplit[0].Trim();
        string actionPart = colonSplit[1].Trim();

        // '?' 기준으로 타이밍과 조건 분리
        string[] questionSplit = timingAndCondition.Split('?');
        data.Timing = (EffectTiming)Enum.Parse(typeof(EffectTiming), questionSplit[0].Trim());

        if (questionSplit.Length > 1) // 조건이 존재한다면
        {
            data.Condition = ParseCondition(questionSplit[1].Trim());
        }

        // '_' 기준으로 효과, 버프종류, 수치 분리 (예: AddBuffPotency_Rupture_3)
        string[] actionSplit = actionPart.Split('_');
        data.Type = (EffectType)Enum.Parse(typeof(EffectType), actionSplit[0].Trim());

        if (actionSplit.Length == 3) // 구조: 효과_버프_수치 (예: AddBuffPotency_Rupture_3)
        {
            data.TargetBuff = (BuffType)Enum.Parse(typeof(BuffType), actionSplit[1].Trim());
            data.Value = int.Parse(actionSplit[2].Trim());
        }
        else if (actionSplit.Length == 2) // 구조: 효과_수치 (예: CoinPowerUp_1) 또는 스킬변이 (TransformSkill_01015)
        {
            // 두 번째 값이 숫자(1015, 1 등)인지, 버프 이름인지 판별
            if (int.TryParse(actionSplit[1].Trim(), out int val))
            {
                data.Value = val;
            }
            else
            {
                // 버프 이름(예: Unique_Concussion)인 경우
                data.TargetBuff = (BuffType)Enum.Parse(typeof(BuffType), actionSplit[1].Trim());
            }
        }

        return data;
    }

    private static ConditionData ParseCondition(string condStr)
    {
        string[] split = condStr.Split('_');
        ConditionType type = (ConditionType)Enum.Parse(typeof(ConditionType), split[0].Trim());

        int targetValue = 0;
        List<BuffType> targetBuffs = new List<BuffType>();

        // BuffSum_1_Rupture_Tremor 같이 값이 뒤에 더 붙어있는 경우
        if (split.Length > 1)
        {
            // 수치(1) 파싱
            if (int.TryParse(split[1].Trim(), out targetValue))
            {
                // 나머지 버프들(Rupture, Tremor) 리스트에 추가
                for (int i = 2; i < split.Length; i++)
                {
                    targetBuffs.Add((BuffType)Enum.Parse(typeof(BuffType), split[i].Trim()));
                }
            }
        }

        return new ConditionData(type, targetValue, targetBuffs.ToArray());
    }
}
