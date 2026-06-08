using System.Collections;
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

    [Header("환경 설정")]
    [SerializeField] private Transform _environmentRoot;

    private List<int> _assignedSinnerIds;
    private int _currentStageId;
    private HashSet<BattleCharacter> _processedCharacters = new HashSet<BattleCharacter>();
    private Dictionary<PlayerCharacter, int> _pendingSkillUses = new Dictionary<PlayerCharacter, int>();

    private GameObject _currentBackgroundInstance;

    private List<PlayerCharacter> _testPlayerList;

    public BattleState CurrentState => _currentState;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;
    }

    void Start()
    {
        StartCoroutine(TestSkillUI());
    }

    private IEnumerator TestSkillUI()
    {
        yield return null;

        PlayerCharacter testPlayer = FindObjectOfType<PlayerCharacter>();
        List<EnemyCharacter> testEnemies = FindObjectsOfType<EnemyCharacter>().ToList();


        if (testPlayer != null)
        {
            IdentityData sampleIdentity = DataManager.Instance.GetIdentity(101);

            _playerCharacters.Clear();
            _enemyCharacters.Clear();
            _playerCharacters.Add(testPlayer);
            _enemyCharacters.AddRange(testEnemies);

            foreach (EnemyCharacter character in testEnemies)
            {
                character.Initialize(sampleIdentity);
                character.DetermineTarget(_playerCharacters.Cast<BattleCharacter>().ToList());
            }

            if (sampleIdentity != null)
            {
                testPlayer.Initialize(sampleIdentity);

                List<PlayerCharacter> playerList = new List<PlayerCharacter> { testPlayer };
                _testPlayerList = playerList;

                List<BattleCharacter> testParticipants = new List<BattleCharacter> { testPlayer };
                testParticipants.AddRange(testEnemies);

                BattleUIManager.Instance.GenerateAllOverheadUIs(testParticipants);

                List<SkillData> sampleSkills = testPlayer.GetSkillQueue(3);

                foreach (EnemyCharacter enemy in testEnemies)
                {
                    for (int i = 0; i < sampleSkills.Count; i++)
                    {
                        BattleUIManager.Instance.UpdateCharacterOverheadSlot(enemy, i, sampleSkills[i]);
                    }

                    if (sampleSkills.Count > 0)
                    {
                        enemy.SetSelectedSkill(sampleSkills[0]);
                    }
                }

                BattleUIManager.Instance.ShowSkillSelectionUI(playerList);

                CustomLogger.LogSystem("[Test] 스킬 UI 및 오버헤드 UI 생성 테스트를 시작합니다.");
            }
        }
    }

    public List<PlayerCharacter> GetPlayerCharacters()
    {
        return _testPlayerList;
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

        var stageData = DataManager.Instance.GetStageData(_currentStageId);

        if (stageData != null)
        {
            if (!string.IsNullOrEmpty(stageData.BackgroundPrefabPath))
            {
                GameObject bgPrefab = Resources.Load<GameObject>(stageData.BackgroundPrefabPath);

                if (bgPrefab != null)
                {
                    Instantiate(bgPrefab, _environmentRoot);
                }
                else
                {
                    CustomLogger.Warn($"배경 프리팹을 찾을 수 없습니다: {stageData.BackgroundPrefabPath}");
                }
            }

            // bgm 재생


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
        }

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

        List<BattleCharacter> allParticipants = new List<BattleCharacter>();
        allParticipants.AddRange(_playerCharacters);
        allParticipants.AddRange(_enemyCharacters);

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.GenerateAllOverheadUIs(allParticipants);

            IdentityData sampleIdentity = DataManager.Instance.GetIdentity(101);
            if (sampleIdentity != null && _playerCharacters.Count > 0)
            {
                List<SkillData> sampleSkills = _playerCharacters[0].GetSkillQueue(3);
                foreach (var enemy in _enemyCharacters.Where(c => c.CurrentHp > 0))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i < sampleSkills.Count)
                        {
                            BattleUIManager.Instance.UpdateCharacterOverheadSlot(enemy, i, sampleSkills[i]);
                        }
                    }

                    if (sampleSkills.Count > 0)
                    {
                        enemy.SetSelectedSkill(sampleSkills[0]);
                    }
                }
            }

            BattleUIManager.Instance.ShowSkillSelectionUI(_playerCharacters);
        }
    }

    private void HandleAction()
    {
        CustomLogger.LogBattle("=== 액션 페이즈 시작 ===");
        _processedCharacters.Clear();

        StartCoroutine(ActionSequenceRoutine());
    }


    private void HandleEnd()
    {
        CustomLogger.LogBattle("턴 종료 페이즈: 버프 갱신 및 상태 체크");

        foreach (var p in _playerCharacters)
        {
            if (p != null)
            {
                p.SetTarget(null);
                p.SetSelectedSkill(null);
            }

        }
        foreach (var e in _enemyCharacters)
        {
            if (e != null)
            {
            e.SetTarget(null);
            e.SetSelectedSkill(null);
            }
        }

        ChangeState(BattleState.SelectSkill);
    }

    public void ChangeEnvironment(string newBgPrefabPath, string newBgmName = null)
    {
        CustomLogger.LogSystem($"[연동] 실시간 환경 전환: {newBgPrefabPath}");

        if (_currentBackgroundInstance != null)
        {
            Destroy(_currentBackgroundInstance);
        }

        if (!string.IsNullOrEmpty(newBgPrefabPath))
        {
            GameObject newBgPrefab = Resources.Load<GameObject>(newBgPrefabPath);
            if (newBgPrefab != null)
            {
                _currentBackgroundInstance = Instantiate(newBgPrefab, _environmentRoot);
            }
            else
            {
                CustomLogger.Warn($"전환할 배경 프리팹을 찾을 수 없습니다: {newBgPrefabPath}");
            }
        }

        // if (!string.IsNullOrEmpty(newBgmName))
        // {
        //     SoundManager.Instance.PlayBGM(newBgmName);
        // }
    }

    private IEnumerator ResolveClashRoutine(BattleCharacter charA, BattleCharacter charB)
    {
        if (charA.SelectedSkill == null || charB.SelectedSkill == null)
        {
            CustomLogger.Warn($"[Clash Error] {charA.name} 또는 {charB.name}의 스킬이 null입니다! 합 연산 취소.");
            yield break;
        }

        CustomLogger.LogBattle($"[Clash] {charA.name} vs {charB.name}");

        int clashCount = 1;

        while (charA.CurrentCoinCount > 0 && charB.CurrentCoinCount > 0)
        {
            CustomLogger.LogBattle($"합 진행 중 {clashCount}합");

            ClashResult resA = ClashEvaluator.CalculateSkillPower(charA, charA.SelectedSkill);
            ClashResult resB = ClashEvaluator.CalculateSkillPower(charB, charB.SelectedSkill);

            yield return new WaitForSeconds(0.5f);

            if (resA.FinalPower > resB.FinalPower)
            {
                CustomLogger.LogBattle($"합 승리: {charA.name} ({resA.FinalPower} > {resB.FinalPower})");
                charB.LoseCoin();
            }
            else if (resB.FinalPower > resA.FinalPower)
            {
                CustomLogger.LogBattle($"합 승리: {charB.name} ({resB.FinalPower} > {resA.FinalPower})");
                charA.LoseCoin();
            }
            else
            {
                CustomLogger.LogBattle($"무승부! ({resA.FinalPower} == {resB.FinalPower}) 코인 파괴 없음");
            }

            CustomLogger.LogBattle($"[Coin] {charA.name}: {charA.CurrentCoinCount}개 / {charB.name}: {charB.CurrentCoinCount}개");

            clashCount++;
            yield return new WaitForSeconds(0.3f);
        }

        if (charA.CurrentCoinCount > 0 && charB.CurrentCoinCount == 0)
        {
            CustomLogger.LogBattle($"[Clash End] {charA.name} 최종 승리! 일방 공격 전환");
            yield return StartCoroutine(PerformOneSidedAttackRoutine(charA, charB, clashCount));
        }
        else if (charB.CurrentCoinCount > 0 && charA.CurrentCoinCount == 0)
        {
            CustomLogger.LogBattle($"[Clash End] {charB.name} 최종 승리! 일방 공격 전환");
            yield return StartCoroutine(PerformOneSidedAttackRoutine(charB, charA, clashCount));
        }
        else
        {
            CustomLogger.LogBattle($"[Clash End] 양측 코인 모두 소진. 타격 없이 종료.");
        }

        _processedCharacters.Add(charA);
        _processedCharacters.Add(charB);
    }

    private IEnumerator PerformOneSidedAttackRoutine(BattleCharacter attacker, BattleCharacter target, int clashCount)
    {
        if (attacker.SelectedSkill == null)
        {
            CustomLogger.Warn($"[Attack Error] {attacker.name}의 스킬이 null입니다! 일방 공격 취소.");
            yield break;
        }

        while (attacker.CurrentCoinCount > 0 && target.CurrentHp > 0)
        {
            ClashResult result = ClashEvaluator.CalculateSkillPower(attacker, attacker.SelectedSkill);

            CustomLogger.LogBattle($"[Attack] {attacker.name}이(가) {target.name}에게 {result.FinalPower} 피해!");

            int clashDamage = Mathf.FloorToInt(result.FinalPower * (clashCount / 10f));

            int totalDamage = result.FinalPower + clashDamage;

            CustomLogger.LogBattle($"[Attack] {attacker.name}이(가) {target.name}에게 {totalDamage} 피해! (기본 {result.FinalPower} + 합 보너스 {clashDamage})");

            target.TakeDamage(totalDamage);

            attacker.UseCoin();

            yield return new WaitForSeconds(0.4f);
        }

        _processedCharacters.Add(attacker);
        yield return null;
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

    public void ConfirmSkillSelection()
    {
        CustomLogger.LogBattle("플레이어 스킬 선택 완료. 액션 페이즈로 진입합니다.");

        foreach (var kvp in _pendingSkillUses)
        {
            kvp.Key.UseSkill(kvp.Value);
        }
        _pendingSkillUses.Clear();

        ChangeState(BattleState.Action);
    }

    public void RegisterAction(PlayerCharacter player, SkillData skill, int slotIndex, EnemyCharacter target, int targetSlotIndex)
    {
        player.SetSelectedSkill(skill);
        player.SetTarget(target);

        _pendingSkillUses[player] = slotIndex;

        CustomLogger.LogBattle($"[Action] {player.name}(슬롯{slotIndex}) -> {target.name}(슬롯{targetSlotIndex}) 합 지정 (스킬: {skill.SkillName})");

        int playerOverheadSlotIndex = 0;
        BattleUIManager.Instance.UpdateCharacterOverheadSlot(player, playerOverheadSlotIndex, skill);

        EventBus<ActionRegisteredEvent>.Publish(new ActionRegisteredEvent
        {
            Attacker = player,
            AttackerSlotIndex = slotIndex,
            Target = target,
            TargetSlotIndex = targetSlotIndex,
            Skill = skill,
        });
    }

    private IEnumerator ActionSequenceRoutine()
    {
        var allParticipants = _playerCharacters.Cast<BattleCharacter>().Concat(_enemyCharacters.Cast<BattleCharacter>()).Where(c => c.CurrentHp > 0).OrderByDescending(c => c.Speed).ToList();

        foreach (var character in allParticipants)
        {
            if (_processedCharacters.Contains(character) || character.CurrentHp <= 0) continue;
            if (character.CurrentTarget == null || character.CurrentTarget.CurrentHp <= 0) continue;

            BattleCharacter target = character.CurrentTarget;

            if (target.CurrentTarget == character)
            {
                yield return StartCoroutine(ResolveClashRoutine(character, target));
            }
            else
            {
                yield return StartCoroutine(PerformOneSidedAttackRoutine(character, target, 0));
            }

            yield return new WaitForSeconds(1.0f);
        }

        CustomLogger.LogBattle("모든 액션 종료. 턴을 마무리합니다.");
        ChangeState(BattleState.End);
    }
}
