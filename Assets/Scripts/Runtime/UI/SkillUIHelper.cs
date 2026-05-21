using UnityEngine;
using UnityEngine.UI;

public struct SkillSpriteSet
{
    public Sprite BaseSprite;
    public Sprite OverlaySprite;
    public Sprite SkillTypeSprite;
}

public static class SkillUIHelper
{
    public static string GetColorName(string sinAttribute)
    {
        return sinAttribute switch
        {
            "분노" => "Red",
            "색욕" => "Orange",
            "나태" => "Yellow",
            "탐식" => "Green",
            "우울" => "Skyblue",
            "오만" => "Blue",
            "질투" => "Purple",
            _ => "Red"
        };
    }

    public static Color GetSinColor(string sinAttribute)
    {
        string hexCode = sinAttribute switch
        {
            "분노" => "#B20000",
            "색욕" => "#D56B00",
            "나태" => "#E2B500",
            "탐식" => "#59B200",
            "우울" => "#00B2B2",
            "오만" => "#1D4678",
            "질투" => "#6C4581",
            _ => "#FFFFFF"
        };

        return GetColorFromHex(hexCode);
    }

    public static string GetSkillType(string skillType)
    {
        return skillType switch
        {
            "참격" => "Slash",
            "타격" => "Hit",
            "관통" => "Pierce",
            _ => "Neutral"
        };
    }

    public static Color GetColorFromHex(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
        {
            return color;
        }
        return Color.white;
    }

    public static Color GetLighterColor(Color color, float satAdjustment, float valAdjustment)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        s *= satAdjustment;
        v *= valAdjustment;

        return Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
    }

    public static void ApplySinColor(string sinAttribute, Image frameOverlayImage)
    {
        Color frameColor = GetSinColor(sinAttribute);

        Color baseColor = GetLighterColor(frameColor, 0.8f, 1.3f);

        if (frameOverlayImage != null) frameOverlayImage.color = frameColor;
    }

    public static SkillSpriteSet GetSkillSprites(SkillData skillData)
    {
        string colorName = GetColorName(skillData.SinAttribute);
        string skillType = GetSkillType(skillData.SkillType);

        string colorSuffix = (skillData.SkillPosition != 4) ? $"_{colorName}" : "";

        string baseName = "Skill_Base";
        string overlayName = $"Skill{skillData.SkillPosition}{colorSuffix}";
        string skillTypeIconName = $"SkillType_{colorName}_{skillType}";

        return new SkillSpriteSet
        {
            BaseSprite = BattleUIManager.Instance.GetFrameSprite(baseName),
            OverlaySprite = BattleUIManager.Instance.GetFrameSprite(overlayName),
            SkillTypeSprite = BattleUIManager.Instance.GetFrameSprite(skillTypeIconName)
        };
    }
}
