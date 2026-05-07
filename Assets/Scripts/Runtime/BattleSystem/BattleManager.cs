using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleManager : MonoSingleton<BattleManager>
{
    [Header("전투 설정")]
    [SerializeField] private BattleState _currentState = BattleState.None;

    [Header("테스트용 씬 참조")]
    [SerializeField] private PlayerCharacter _testPlayer;
    [SerializeField] private EnemyCharacter _testEnemy;

    public BattleState CurrentState => _currentState;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }

    void Start()
    {
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
        CustomLogger.LogBattle("전투 환경 초기화 중...");

        // 🌟 2. DataManager에서 '이상'의 데이터(ID: 1)를 가져옵니다.
        // (주의: 딕셔너리 이름 IdentityDict, SkillDict는 DataManager에 작성하신 실제 변수명으로 맞춰주세요!)
        if (DataManager.Instance.IdentityTable.TryGetValue(101, out IdentityData yiSangData))
        {
            // 아군 초기화
            _testPlayer.Initialize(yiSangData);
            CustomLogger.LogSystem($"[Test] 아군 {_testPlayer.gameObject.name} 세팅 완료.");

            // 테스트를 위해 '이상'의 첫 번째 스킬을 강제로 장착
            if (yiSangData.SkillIDs.Count > 0 && DataManager.Instance.SkillTable.TryGetValue(yiSangData.SkillIDs[0], out SkillData yiSangSkill))
            {
                _testPlayer.SetSelectedSkill(yiSangSkill);
            }
        }

        // 🌟 3. 적군 데이터 세팅
        // (만약 아직 DataManager에 EnemyData 파싱 로직이 없다면, 임시로 2번 인격 데이터를 적에게 넣어줍니다)
        if (DataManager.Instance.IdentityTable.TryGetValue(101, out IdentityData dummyEnemyData))
        {
            _testEnemy.Initialize(dummyEnemyData);

            // 적도 테스트를 위해 임시 스킬 하나 쥐어주기
            if (dummyEnemyData.SkillIDs.Count > 0 &&
                DataManager.Instance.SkillTable.TryGetValue(dummyEnemyData.SkillIDs[0], out SkillData enemySkill))
            {
                _testEnemy.SetSelectedSkill(enemySkill);
            }
        }

        // 초기화 완료 후 스킬 선택 페이즈로 전환
        ChangeState(BattleState.SelectSkill);
    }

    private void HandleSelectSkill()
    {

    }

    private void HandleAction()
    {
        CustomLogger.LogBattle("액션 페이즈 시작: 속도 순 정렬 및 합 계산");

        var allCharacter = FindObjectsOfType<BattleCharacter>().OrderByDescending(c => c.Speed).ToList();

        foreach (var character in allCharacter)
        {
            if (character.CurrentHp <= 0 || character.CurrentTarget == null) continue;

            if (character.CurrentTarget.CurrentTarget == character)
            {
                ResolveClash(character, character.CurrentTarget);
            }
            else
            {
                PerformOneSidedAttack(character, character.CurrentTarget);
            }
        }

        ChangeState(BattleState.SelectSkill);
    }

    private void HandleEnd()
    {

    }

    private void ResolveClash(BattleCharacter a, BattleCharacter b)
    {
        ClashResult resultA = ClashEvaluator.CalculateSkillPower(a, a.SelectedSkill);
        ClashResult resultB = ClashEvaluator.CalculateSkillPower(b, b.SelectedSkill);

        if (resultA.FinalPower > resultB.FinalPower)
        {
            b.LoseCoin();
        }
        else if (resultA.FinalPower < resultB.FinalPower)
        {
            a.LoseCoin();
        }
        else
        {

        }
    }

    private void PerformOneSidedAttack(BattleCharacter attacker, BattleCharacter target)
    {
        var result = attacker.GetCurrentSkillPower(attacker.SelectedSkill);
        target.TakeDamage(result.FinalPower);
        CustomLogger.LogBattle($"{attacker.name}의 일방 공격! {target.name}에게 {result.FinalPower} 피해.");
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
