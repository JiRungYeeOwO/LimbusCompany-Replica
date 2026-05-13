using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 연결")]
    [SerializeField] private Image _frameBaseImage;
    [SerializeField] private Image _baseMaskImage;
    [SerializeField] private Image _frameOverlayImage;
    [SerializeField] private Image _skillIconImage;

    private SkillData _currentSkillData;
    private PlayerCharacter _ownerCharacter;

    public void SetupSlot(PlayerCharacter owner, SkillData skillData)
    {
        _ownerCharacter = owner;
        _currentSkillData = skillData;

        string colorName = GetColorName(skillData.SinAttribute);

        string colorSuffix = (skillData.SkillPosition != 4) ? $"_{colorName}" : "";

        string frameBaseName = $"Skill{skillData.SkillPosition}{colorSuffix}_Base";
        string baseName = "Skill_Base";
        string overlayName = $"Skill{skillData.SkillPosition}{colorSuffix}";

        Sprite frameBaseSprite = BattleUIManager.Instance.GetFrameSprite(frameBaseName);
        Sprite baseSprite = BattleUIManager.Instance.GetFrameSprite(baseName);
        Sprite overlaySprite = BattleUIManager.Instance.GetFrameSprite(overlayName);

        if (_frameBaseImage != null)
        {
            _frameBaseImage.sprite = frameBaseSprite;
        }

        if (_baseMaskImage != null)
        {
            _baseMaskImage.sprite = baseSprite;
        }

        if (_frameOverlayImage != null)
        {
            _frameOverlayImage.sprite = overlaySprite;
        }

        ApplySinColor(skillData.SinAttribute);

        _skillIconImage.sprite = BattleUIManager.Instance.GetSkillIconSprite(skillData.SkillID);

        GetComponent<RectTransform>().localRotation = Quaternion.Euler(-45f, 0f, 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentSkillData == null || _ownerCharacter == null) return;

        BattleUIManager.Instance.OnSkillSelected(_ownerCharacter, _currentSkillData);
    }

    private string GetColorName(string sinAttribute)
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

    private Color GetSinColor(string sinAttribute)
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

    private Color GetColorFromHex(string hexCode)
    {
        if (ColorUtility.TryParseHtmlString(hexCode, out Color color))
        {
            return color;
        }
        return Color.white;
    }

    private void ApplySinColor(string sinAttribute)
    {
        Color frameColor = GetSinColor(sinAttribute);

        Color baseColor = GetLighterColor(frameColor, 0.8f, 1.3f);

        if (_frameBaseImage != null) _frameBaseImage.color = baseColor;
        if (_frameOverlayImage != null) _frameOverlayImage.color = frameColor;
    }

    // 🛠️ 색상을 밝게 만들어주는 헬퍼 함수
    private Color GetLighterColor(Color color, float satAdjustment, float valAdjustment)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        s *= satAdjustment;
        v *= valAdjustment;

        return Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
    }
}
