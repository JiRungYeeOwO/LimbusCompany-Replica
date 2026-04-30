using System.Collections.Generic;
using UnityEngine;

public abstract class BattleCharacter : MonoBehaviour
{
    [Header("캐릭터 데이터")]
    [SerializeField] private CharacterBaseData _characterData;

    [Header("현재 스테이터스")]
    [SerializeField] private int _currentHp;
    [SerializeField] private int _currentSp;

    [Header("현재 버프 상태")]
    [SerializeField] private Dictionary<BuffType, int> _activeBuffs = new Dictionary<BuffType, int>();

    public CharacterBaseData CharacterData => _characterData;
    public int CurrentHp => _currentHp;
    public int CurrentSp => _currentSp;

    public int Speed {  get; private set; }
    public int MaxHp => _characterData != null ? _characterData.MaxHP : 0;

    public virtual void Initialize(IdentityData data)
    {
        _characterData = data;
        _currentHp = MaxHp;
        _currentSp = 0;

        _activeBuffs.Clear();
        CustomLogger.LogBattle($"{gameObject.name} 초기화 완료 (HP: {_currentHp}/{MaxHp})");
    }

    public virtual void TakeDamage(int damage)
    {
        _currentHp = Mathf.Max(0, _currentHp - damage);

        CustomLogger.LogBattle($"{gameObject.name} 피격! {damage} 피해 (남은 체력: {_currentHp}/{MaxHp})");

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        CustomLogger.LogBattle($"{gameObject.name} 사망");
    }


    public void RollSpeed()
    {
        if (_characterData == null) return;

        var (min, max) = _characterData.GetSpeed();

        Speed = Random.Range(min, max + 1);

        CustomLogger.LogSystem($"[Battle] {gameObject.name} 속도 굴림: {Speed} ({min}~{max})");
    }

    public abstract void DetermineTarget();
    
    public int GetBuffValue(BuffType buff)
    {
        if (_activeBuffs.TryGetValue(buff, out int value)) return value;
        return 0;
    }

    public void AddBuff(BuffType buff, int value)
    {
        if (_activeBuffs.ContainsKey(buff)) _activeBuffs[buff] += value;
        else _activeBuffs[buff] = value;
    }

    public void DecreaseBuff(BuffType buff, int value)
    {
        if (!_activeBuffs.ContainsKey(buff)) return;

        _activeBuffs[buff] -= value;

        if (_activeBuffs[buff] <= 0) _activeBuffs.Remove(buff);
    }
}
