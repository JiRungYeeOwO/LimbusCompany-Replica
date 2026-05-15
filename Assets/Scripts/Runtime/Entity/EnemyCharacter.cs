using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : BattleCharacter
{
    private MeshRenderer _renderer;
    private Color _originColor;

    private void Awake()
    {
        if (IsTestEnvironment)
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer != null) _originColor = _renderer.material.color;
        }
    }

    public override void Initialize(CharacterBaseData data)
    {
        base.Initialize(data);

        if (data is EnemyData enemyData)
        {
            CustomLogger.LogSystem($"[Enemy] 적 개체 {enemyData.Name}");
        }
    }

    public override void DetermineTarget(List<BattleCharacter> players)
    {
        if (players == null || players.Count == 0) return;

        foreach (var player in players)
        {
            if (player.GetBuffValue(BuffType.Provoke) > 0)
            {
                CurrentTarget = player;
                CustomLogger.LogBattle($"[Enemy] {gameObject.name} -> 도발에 끌림: {CurrentTarget.gameObject.name}");
                return;
            }
        }

        int randomIdx = Random.Range(0, players.Count);
        CurrentTarget = players[randomIdx];

        CustomLogger.LogBattle($"[Enemy] {gameObject.name} -> 기본 패턴 타겟 지정: {CurrentTarget.gameObject.name}");
    }

    public void SetHighlight(bool isActive)
    {
        if (_renderer == null) return;
        _renderer.material.color = isActive ? Color.yellow : _originColor;
    }
}
