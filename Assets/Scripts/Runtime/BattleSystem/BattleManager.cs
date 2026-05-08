using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoSingleton<BattleManager>
{
    [Header("상태 관리")]
    [SerializeField] private BattleState _currentState = BattleState.None;

    [Header("프리팹 및 슬롯 설정")]
    [SerializeField] private GameObject _sinnerPrefab;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform[] _sinnerSlots;
    [SerializeField] private Transform[] _enemySlots;

    [Header("전투 참가자 목록")]
    [SerializeField] private List<PlayerCharacter> _playerCharacters = new List<PlayerCharacter>();
    [SerializeField] private List<EnemyCharacter> _enemyCharacters = new List<EnemyCharacter>();

    private List<int> _assignedSinnerIds;
    private int _currentStageId;
    private HashSet<BattleCharacter> _processedCharacters = new HashSet<BattleCharacter>();

    public BattleState CurrentState => _currentState;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }

    void Start()
    {
        
    }

    public void PrepareBattle(List<int> sinnerIds, int stageId)
    {
        _assignedSinnerIds = sinnerIds;
        _currentStageId = stageId;
        ChangeState(BattleState.Init);
    }

    public void ChangeState(BattleState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;
        CustomLogger.LogBattle($"[BattleManager] 상태 변경: {newState}");

        switch (_currentState)
        {
            case BattleState.Init:
                HandleInit();
                break;
            case BattleState.SelectSkill:
                HandleSelectSkill();
                break;
            case BattleState.Action:
                HandleAction(); 
                break;
            case BattleState.End:
                HandleEnd();
                break;
        }
    }

    private void HandleInit()
    {
        CustomLogger.LogBattle("전투 초기화 시작: 캐릭터 생성 및 데이터 주입");

        ClearBattlefield();

        // 1. 아군 생성 (편성 순서대로 슬롯에 배치)
        for (int i = 0; i < _assignedSinnerIds.Count; i++)
        {
            if (i >= _sinnerSlots.Length) break;

            GameObject go = Instantiate(_sinnerPrefab, _sinnerSlots[i].position, Quaternion.identity);
            go.name = $"Player_{i}";
            PlayerCharacter pc = go.GetComponent<PlayerCharacter>();

            IdentityData data = DataManager.Instance.GetIdentity(_assignedSinnerIds[i]);
            pc.Initialize(data);
            _playerCharacters.Add(pc);
        }

        // 2. 적군 생성 (스테이지 데이터 기반)
        var stageData = DataManager.Instance.GetStageData(_currentStageId);
        foreach (var spawnInfo in stageData.EnemyList)
        {
            int slotIdx = spawnInfo.FormationPos;
            if (slotIdx >= _enemySlots.Length) continue;

            GameObject go = Instantiate(_enemyPrefab, _enemySlots[slotIdx].position, Quaternion.identity);
            go.name = $"Enemy_{slotIdx}";
            EnemyCharacter ec = go.GetComponent<EnemyCharacter>();

            EnemyData data = DataManager.Instance.GetEnemy(spawnInfo.EnemyID);
            ec.Initialize(data);
            _enemyCharacters.Add(ec);
        }

        ChangeState(BattleState.SelectSkill);
    }

    private void HandleSelectSkill()
    {
        CustomLogger.LogBattle("스킬 선택 페이즈: 속도 주사위 굴림");

        foreach (var p in _playerCharacters.Where(c => c.CurrentHp > 0))
        {
            p.RollSpeed();
        }

        foreach (var e in _enemyCharacters.Where(c => c.CurrentHp > 0))
        {
            e.RollSpeed();
        }

        foreach (var e in _enemyCharacters.Where(c => c.CurrentHp > 0))
        {
            e.DetermineTarget(_playerCharacters.Cast<BattleCharacter>().ToList());
        }
    }

    private void HandleAction()
    {
        CustomLogger.LogBattle("=== 액션 페이즈 시작 ===");
        _processedCharacters.Clear();

        var allParticipants = _playerCharacters.Cast<BattleCharacter>().Concat(_enemyCharacters.Cast<BattleCharacter>()).Where(c => c.CurrentHp > 0).OrderByDescending(c => c.Speed).ToList();

        foreach (var character in allParticipants)
        {
            if (_processedCharacters.Contains(character) || character.CurrentHp <= 0) continue;
            if (character.CurrentTarget == null || character.CurrentTarget.CurrentHp <= 0) continue;

            BattleCharacter target = character.CurrentTarget;

            if (target.CurrentTarget == character)
            {
                ResolveClash(character, target);
            }
            else
            {
                PerformOneSidedAttack(character, target);
            }
        }
    }

    private void HandleEnd()
    {
        CustomLogger.LogBattle("턴 종료 페이즈: 버프 갱신 및 상태 체크");
    }

    private void ResolveClash(BattleCharacter charA, BattleCharacter charB)
    {
        CustomLogger.LogBattle($"[Clash] {charA.name} vs {charB.name}");

        ClashResult resA = ClashEvaluator.CalculateSkillPower(charA, charA.SelectedSkill);
        ClashResult resB = ClashEvaluator.CalculateSkillPower(charB, charB.SelectedSkill);

        if (resA.FinalPower > resB.FinalPower)
        {
            CustomLogger.LogBattle($"{charA.name} 승리!");
            charB.LoseCoin();
            PerformOneSidedAttack(charA, charB);
        }
        else if (resB.FinalPower > resA.FinalPower)
        {
            CustomLogger.LogBattle($"{charB.name} 승리!");
            charA.LoseCoin();
            PerformOneSidedAttack(charB, charA);
        }
        else
        {
            CustomLogger.LogBattle("무승부 (재합 로직 필요)");
        }

        _processedCharacters.Add(charA);
        _processedCharacters.Add(charB);
    }

    private void PerformOneSidedAttack(BattleCharacter attacker, BattleCharacter target)
    {
        ClashResult result = ClashEvaluator.CalculateSkillPower(attacker, attacker.SelectedSkill);
        target.TakeDamage(result.FinalPower);
        _processedCharacters.Add(attacker);
    }

    private void ClearBattlefield()
    {
        foreach (var p in _playerCharacters)
        {
            if (p != null)
            {
                Destroy(p.gameObject);
            }
        }

        foreach (var e in _enemyCharacters)
        {
            if (e != null)
            {
                Destroy(e.gameObject);
            }
        }

        _playerCharacters.Clear();
        _enemyCharacters.Clear();
    }

    public void ResetBattle()
    {
        CustomLogger.LogSystem("전투를 초기화합니다.");
        ChangeState(BattleState.Init);
    }

    public void SkipToNextPhase()
    {
        BattleState nextState = (BattleState)(((int)_currentState + 1) % 5);

        if (nextState == BattleState.None)
        {
            nextState = BattleState.Init;
        }

        ChangeState(nextState);
    }
}
