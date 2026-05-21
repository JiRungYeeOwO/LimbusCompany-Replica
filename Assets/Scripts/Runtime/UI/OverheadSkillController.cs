using System.Collections.Generic;
using UnityEngine;

public class OverheadSkillController : MonoBehaviour
{
    [Header("스킬 설정")]
    [SerializeField] private GameObject _overheadSlotPrefab;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private Vector3 _offset = new Vector3(0, 2.5f, 0);

    private BattleCharacter _targetCharacter;
    private RectTransform _rectTransform;
    private RectTransform _parentCanvasRect;

    private List<OverheadSkillUI> _slots = new List<OverheadSkillUI>();

    private void LateUpdate()
    {
        if (_targetCharacter == null || _targetCharacter.CurrentHp <= 0)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 worldPos = _targetCharacter.transform.position + _offset;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentCanvasRect, screenPos, Camera.main, out Vector2 localPos);

        _rectTransform.anchoredPosition = localPos;
    }

    public void Initialize(BattleCharacter character, int slotCount)
    {
        _targetCharacter = character;
        _rectTransform = GetComponent<RectTransform>();

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            _parentCanvasRect = parentCanvas.GetComponent<RectTransform>();
        }

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotGO = Instantiate(_overheadSlotPrefab, _slotContainer);
            OverheadSkillUI slotUI = slotGO.GetComponent<OverheadSkillUI>();
            if (slotUI != null)
            {
                slotUI.Initialize(character, i);
                _slots.Add(slotUI);
            }
        }
    }

    public void SetSkill(int slotIndex, SkillData skill)
    {
        if (slotIndex >= 0 && slotIndex < _slots.Count)
        {
            _slots[slotIndex].UpdateSkillUI(skill);
        }
        else
        {
            CustomLogger.Warn($"[OverheadUI] {_targetCharacter.name}의 {slotIndex}번 슬롯을 찾을 수 없습니다. (현재 생성된 슬롯 수: {_slots.Count})");
        }
    }
}
