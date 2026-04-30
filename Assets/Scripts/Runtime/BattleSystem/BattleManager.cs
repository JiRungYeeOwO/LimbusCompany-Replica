using UnityEngine;

public class BattleManager : MonoSingleton<BattleManager>
{
    [Header("전투 설정")]
    [SerializeField] private BattleState _currentState = BattleState.None;

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

    }

    private void HandleSelectSkill()
    {

    }

    private void HandleAction()
    {

    }

    private void HandleEnd()
    {

    }
}
