using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetingHandler : MonoSingleton<TargetingHandler>
{
    [Header("화살표 연출")]
    [SerializeField] private RectTransform _arrowPrefab;
    [SerializeField] private RectTransform _lineContainer;

    [Header("테스트 용 설정")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private bool _isTestEnvironment = false;

    private EnemyCharacter _currentHoveredEnemy;

    private SkillSlotUI _activeSlot;
    private PlayerCharacter _activePlayer;
    private SkillData _activeSkill;
    private int _activeSlotIndex;
    private bool _isDragging;

    private RectTransform _currentArrow;
    private Vector2 _localStartPos;

    private void OnEnable()
    {
        EventBus<SkillDragStartedEvent>.Subscribe(OnSkillDragStarted);
        EventBus<SkillDragUpdatedEvent>.Subscribe(OnSkillDragUpdate);
        EventBus<SkillDragEndedEvent>.Subscribe(OnSkillDragEnd);
    }

    private void OnDisable()
    {
        EventBus<SkillDragStartedEvent>.Unsubscribe(OnSkillDragStarted);
        EventBus<SkillDragUpdatedEvent>.Unsubscribe(OnSkillDragUpdate);
        EventBus<SkillDragEndedEvent>.Unsubscribe(OnSkillDragEnd);
    }

    private void OnSkillDragStarted(SkillDragStartedEvent e)
    {
        _isDragging = true;
        _activeSlot = e.Slot;
        _activePlayer = e.Player;
        _activeSkill = e.Skill;
        _activeSlotIndex = e.Index;

        if (_currentArrow == null)
        {
            _currentArrow = Instantiate(_arrowPrefab, _lineContainer);
        }
        _currentArrow.gameObject.SetActive(true);

        Vector2 screenPoint = Camera.main.WorldToScreenPoint(e.Slot.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_lineContainer, screenPoint, Camera.main, out _localStartPos);

        _currentArrow.anchoredPosition = _localStartPos;
        _currentArrow.localPosition = new Vector3(_currentArrow.localPosition.x, _currentArrow.localPosition.y, 0f); // 깊이만 0으로
        _currentArrow.sizeDelta = new Vector2(0, _currentArrow.sizeDelta.y);

        if (_isTestEnvironment)
        {
            _activeSlot.SetHighlight(true);
        }

        CustomLogger.LogSystem($"[TargetingHandler] {e.Skill.SkillName} 타겟팅 시작");
        // TODO: 화살표 시각적 활성화 (TargetingLine.Show)
    }

    private void OnSkillDragUpdate(SkillDragUpdatedEvent e)
    {
        if (!_isDragging || _currentArrow == null) return;

        EnemyCharacter target = DetectEnemyAtMouse(e.MousePos);
        if (target != _currentHoveredEnemy)
        {
            if (_currentHoveredEnemy != null) _currentHoveredEnemy.SetHighlight(false);
            _currentHoveredEnemy = target;
            if (_currentHoveredEnemy != null) _currentHoveredEnemy.SetHighlight(true);
        }

        Vector2 finalTargetScreenPos = e.MousePos;
        if (_currentHoveredEnemy != null && _isTestEnvironment)
        {
            finalTargetScreenPos = Camera.main.WorldToScreenPoint(_currentHoveredEnemy.transform.position);
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_lineContainer, finalTargetScreenPos, Camera.main, out Vector2 localTargetPos);

        Vector2 direction = localTargetPos - _localStartPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        _currentArrow.sizeDelta = new Vector2(distance, _currentArrow.sizeDelta.y);
        _currentArrow.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnSkillDragEnd(SkillDragEndedEvent e)
    {
        if (!_isDragging) return;
        _isDragging = false;

        OverheadSkillUI targetSlot = DetectOverheadSlotAtMouse(e.MousePos);

        if (targetSlot != null)
        {
            EnemyCharacter targetEnemy = targetSlot.OwnerCharacter as EnemyCharacter;
            int targetSlotIndex = targetSlot.SlotIndex;

            BattleManager.Instance.RegisterAction(_activePlayer, _activeSkill, _activeSlotIndex, targetEnemy, targetSlotIndex);
        }
        else
        {
            EnemyCharacter targetEnemy3D = DetectEnemyAtMouse(e.MousePos);

            if (targetEnemy3D != null)
            {
                int fallbackSlotIndex = 0;

                BattleManager.Instance.RegisterAction(_activePlayer, _activeSkill, _activeSlotIndex, targetEnemy3D, fallbackSlotIndex);

                CustomLogger.LogSystem($"[TargetingHandler] 3D 오브젝트 직접 타겟팅 성공 (임시 0번 슬롯 배정)");
            }
        }

        if (_currentArrow != null)
        {
            _currentArrow.gameObject.SetActive(false);
        }

        if (_isTestEnvironment)
        {
            if (_activeSlot != null) _activeSlot.SetHighlight(false);
            if (_currentHoveredEnemy != null) _currentHoveredEnemy.SetHighlight(false);

            _currentHoveredEnemy = null;
        }

        CustomLogger.LogSystem($"[TargetingHandler] 타겟팅 종료");
    }

    private EnemyCharacter DetectEnemyAtMouse(Vector2 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _enemyLayer))
        {
            return hit.collider.GetComponent<EnemyCharacter>();
        }
        return null;
    }

    private OverheadSkillUI DetectOverheadSlotAtMouse(Vector2 mousePos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            OverheadSkillUI slot = result.gameObject.GetComponentInParent<OverheadSkillUI>();

            if (slot != null && slot.OwnerCharacter is EnemyCharacter)
            {
                return slot;
            }
        }
        return null;
    }
}
