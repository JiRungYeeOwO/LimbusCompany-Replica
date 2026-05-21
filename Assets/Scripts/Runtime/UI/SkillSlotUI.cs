using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 연결")]
    [SerializeField] private Image _baseMaskImage;
    [SerializeField] private Image _frameOverlayImage;
    [SerializeField] private Image _skillIconImage;
    [SerializeField] private Image _skillTypeImage;

    private SkillData _currentSkillData;
    private PlayerCharacter _ownerCharacter;

    private int _slotIndex;

    private Canvas _localCanvas;
    private bool _isDraggingThis = false;
    private Vector3 _originalLocalScale;

    private void Awake()
    {
        _localCanvas = GetComponent<Canvas>();
        if (_localCanvas == null)
        {
            _localCanvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public void SetupSlot(PlayerCharacter owner, SkillData skillData, bool isNextSkill, int index)
    {
        _ownerCharacter = owner;
        _currentSkillData = skillData;
        _slotIndex = index;

        SkillSpriteSet sprites = SkillUIHelper.GetSkillSprites(skillData);

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

            if (SkillUIHelper.GetSkillType(skillData.SkillType) == "Neutral")
            {
                _skillTypeImage.enabled = false;
            }
            else
            {
                _skillTypeImage.enabled = true;
                _skillTypeImage.color = Color.white;
            }
        }    

        SkillUIHelper.ApplySinColor(skillData.SinAttribute, _frameOverlayImage);

        _skillIconImage.sprite = BattleUIManager.Instance.GetSkillIconSprite(skillData.SkillID);

        GetComponent<RectTransform>().localRotation = Quaternion.Euler(-75f, 0f, 0f);

        if (isNextSkill)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;

            _frameOverlayImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _baseMaskImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _skillIconImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
            _skillTypeImage.color *= new Color(0.3f, 0.3f, 0.3f, 0.4f);
        }
    }

    private void ApplySinColor(string sinAttribute)
    {
        Color frameColor = SkillUIHelper.GetSinColor(sinAttribute);

        Color baseColor = SkillUIHelper.GetLighterColor(frameColor, 0.8f, 1.3f);

        if (_frameOverlayImage != null) _frameOverlayImage.color = frameColor;
    }

    private void BringToFront()
    {
        if (_localCanvas != null)
        {
            _localCanvas.overrideSorting = true;
            _localCanvas.sortingOrder = 100;

            _originalLocalScale = gameObject.transform.localScale;

            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void SendToBack()
    {
        if (_localCanvas != null)
        {
            _localCanvas.overrideSorting = false;
            _localCanvas.sortingOrder = 0;

            gameObject.transform.localScale = _originalLocalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentSkillData == null) return;

        BringToFront();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_currentSkillData == null) return;

        if (!_isDraggingThis)
        {
            SendToBack();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentSkillData == null || _currentSkillData.SkillPosition == 4) return;

        _isDraggingThis = true;
        BringToFront();

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

            _frameOverlayImage.color *= dimFactor;
            _baseMaskImage.color *= dimFactor;
            _skillIconImage.color *= dimFactor;
            _skillTypeImage.color *= dimFactor;
        }
        else
        {
            ApplySinColor(_currentSkillData.SinAttribute);

            _skillIconImage.color = Color.white;
            _baseMaskImage.color = Color.white;
            _skillTypeImage.color = Color.white;
        }
    }
}
