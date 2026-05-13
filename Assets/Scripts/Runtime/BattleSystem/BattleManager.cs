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

    private GameObject _currentBackgroundInstance;

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
        yield return null; // DataManager.Awake가 끝날 때까지 대기

        // 2. 씬에 있는 임시 플레이어 오브젝트를 찾습니다.
        PlayerCharacter testPlayer = FindObjectOfType<PlayerCharacter>();

        if (testPlayer != null)
        {
            // 3. 데이터 매니저에서 테스트할 인격 데이터(예: ID 1번)를 가져옵니다.
            // CSV 파일(Identities.csv)의 첫 번째 열에 있는 ID를 넣으세요.
            IdentityData sampleIdentity = DataManager.Instance.GetIdentity(101);

            if (sampleIdentity != null)
            {
                // 4. 캐릭터 초기화 (이때 비로소 GetSkillList가 작동할 준비가 됨)
                testPlayer.Initialize(sampleIdentity);

                // 5. UI 매니저에게 리스트를 넘겨서 화면에 그리게 함
                List<PlayerCharacter> playerList = new List<PlayerCharacter> { testPlayer };
                BattleUIManager.Instance.ShowSkillSelectionUI(playerList);

                CustomLogger.LogSystem("[Test] 스킬 UI 생성 테스트를 시작합니다.");
            }
        }
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

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowSkillSelectionUI(_playerCharacters);
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

    public void ConfirmSkillSelection()
    {
        CustomLogger.LogBattle("플레이어 스킬 선택 완료. 액션 페이즈로 진입합니다.");
        ChangeState(BattleState.Action);
    }
}
