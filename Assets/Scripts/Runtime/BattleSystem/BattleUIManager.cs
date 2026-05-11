using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoSingleton<BattleUIManager>
{
    [Header("UI 패널")]
    [SerializeField] private GameObject _skillSelectionPanel;
    [SerializeField] private GameObject _actionTimelinePanel;

    private PlayerCharacter _currentlySelectingPlayer;
    private SkillData _selectedSkillInfo;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ShowSkillSelectionUI(List<PlayerCharacter> players)
    {
        _actionTimelinePanel.SetActive(false);
        _skillSelectionPanel.SetActive(true);
        CustomLogger.LogSystem("[UI] 스킬 선택 UI 활성화. 플레이어 입력을 대기합니다.");
    }

    public void ShowActionTimelineUI(List<BattleCharacter> sortedParticipants)
    {
        _skillSelectionPanel.SetActive(false);
        _actionTimelinePanel.SetActive(true);

        CustomLogger.LogSystem("[UI] 액션 타임라인 패널 활성화.");
    }

    public void OnSkillSelected(PlayerCharacter owner, SkillData skill)
    {
        _currentlySelectingPlayer = owner;
        _selectedSkillInfo = skill;
        CustomLogger.LogSystem($"[UI] {_currentlySelectingPlayer.gameObject.name}의 '{skill.SkillName}' 스킬 선택됨. 타겟을 지정해주세요.");
    }

    public void OnTargetSelected(EnemyCharacter target)
    {
        if (_currentlySelectingPlayer == null || _selectedSkillInfo == null) return;

        _currentlySelectingPlayer.SetSelectedSkill(_selectedSkillInfo);
        _currentlySelectingPlayer.SetTarget(target);

        CustomLogger.LogSystem($"[UI] {_currentlySelectingPlayer.gameObject.name} -> {target.gameObject.name} 타겟팅 완료!");

        _currentlySelectingPlayer = null;
        _selectedSkillInfo = null;
    }

    public void OnClickStartClashButton()
    {
        gameObject.SetActive(false);
        BattleManager.Instance.ConfirmSkillSelection();
    }
}
