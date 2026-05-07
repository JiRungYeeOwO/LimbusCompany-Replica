using System.Collections.Generic;
using UnityEngine;

public class ClashResult
{
    public int FinalPower;
    public List<bool> CoinResults = new List<bool>();
}

public static class ClashEvaluator
{
    public static ClashResult CalculateSkillPower(BattleCharacter character, SkillData skill)
    {
        ClashResult result = new ClashResult();
        result.FinalPower = skill.BasePower;

        float headsProbability = 50f + character.CurrentSp;
        headsProbability = Mathf.Clamp(headsProbability, 5f, 95f);

        for (int i = 0; i < skill.CoinCount; i++)
        {
            float roll = Random.Range(0f, 100f);
            bool isHead = roll <= headsProbability;

            result.CoinResults.Add(isHead);

            if (isHead)
            {
                result.FinalPower += skill.CoinPower;
            }
        }

        return result;
    }
}
