using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("UI 연결")]
    [SerializeField] private Image _frameBaseImage;
    [SerializeField] private Image _baseMaskImage;
    [SerializeField] private Image _frameOverlayImage;
    [SerializeField] private Image _skillIconImage;

    private SkillData _currentSkillData;
    private PlayerCharacter _ownerCharacter;

    private int _slotIndex;

    public void SetupSlot(PlayerCharacter owner, SkillData skillData, bool isNextSkill, int index)
    {
        _ownerCharacter = owner;
        _currentSkillData = skillData;
        _slotIndex = index;

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

        GetComponent<RectTransform>().localRotation = Quaternion.Euler(-75f, 0f, 0f);

        if (isNextSkill)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;

            _frameBaseImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _frameOverlayImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _baseMaskImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _skillIconImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
        }
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

    private Color GetLighterColor(Color color, float satAdjustment, float valAdjustment)
    {
        float h, s, v;
        Color.RGBToHSV(color, out h, out s, out v);

        s *= satAdjustment;
        v *= valAdjustment;

        return Color.HSVToRGB(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentSkillData == null || _currentSkillData.SkillPosition == 4) return;

        EventBus<SkillDragStartedEvent>.Publish(new SkillDragStartedEvent
        {
            Slot = this,
            Player = _ownerCharacter,
            Skill = _currentSkillData,
            Index = _slotIndex
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        EventBus<SkillDragUpdatedEvent>.Publish(new SkillDragUpdatedEvent
        {
            MousePos = eventData.position
        });
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EventBus<SkillDragEndedEvent>.Publish(new SkillDragEndedEvent
        {
            MousePos = eventData.position
        });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentSkillData == null || _ownerCharacter == null) return;

        EventBus<SKillSelectedEvent>.Publish(new SKillSelectedEvent
        {
            Player = _ownerCharacter,
            Skill = _currentSkillData,
            Index = _slotIndex
        });
    }

    public void SetHighlight(bool isActive)
    {
        if (isActive)
        {
            Color dimFactor = new Color(0.3f, 0.3f, 0.3f, 1f);

            _frameBaseImage.color *= dimFactor;
            _frameOverlayImage.color *= dimFactor;
            _baseMaskImage.color *= dimFactor;
            _skillIconImage.color *= dimFactor;
        }
        else
        {
            ApplySinColor(_currentSkillData.SinAttribute);

            _skillIconImage.color = Color.white;
            _baseMaskImage.color = Color.white;
        }
    }
}
