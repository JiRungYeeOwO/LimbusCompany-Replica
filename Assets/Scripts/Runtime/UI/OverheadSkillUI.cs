using UnityEngine;
using UnityEngine.UI;

public class OverheadSkillUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image _baseMaskImage;
    [SerializeField] private Image _frameOverlayImage;
    [SerializeField] private Image _skillIconImage;
    [SerializeField] private Image _skillTypeImage;

    private BattleCharacter _ownerCharacter;

    public BattleCharacter OwnerCharacter => _ownerCharacter;
    public int SlotIndex { get; private set; }

    public void Initialize(BattleCharacter ownerCharacter, int slotIndex)
    {
        _ownerCharacter = ownerCharacter;
        SlotIndex = slotIndex;
        _skillTypeImage.enabled = false;
    }

    public void UpdateSkillUI(SkillData skill)
    {
        _skillIconImage.enabled = true;
        _skillTypeImage.enabled = true;

        SkillSpriteSet sprites = SkillUIHelper.GetSkillSprites(skill);

        if (_baseMaskImage != null)
        {
            _baseMaskImage.sprite = sprites.BaseSprite;
        }

        if (_frameOverlayImage != null)
        {
            _frameOverlayImage.sprite = sprites.OverlaySprite;
        }

        if (_skillTypeImage != null)
        {
            _skillTypeImage.sprite = sprites.SkillTypeSprite;

            if (SkillUIHelper.GetSkillType(skill.SkillType) == "Neutral")
            {
                _skillTypeImage.enabled = false;
            }
            else
            {
                _skillTypeImage.enabled = true;
                _skillTypeImage.color = Color.white;
            }
        }

        SkillUIHelper.ApplySinColor(skill.SinAttribute, _frameOverlayImage);

        _skillIconImage.sprite = BattleUIManager.Instance.GetSkillIconSprite(skill.SkillID);
        _skillIconImage.color = Color.white;
    }
}
