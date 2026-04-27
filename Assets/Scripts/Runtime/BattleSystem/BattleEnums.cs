public enum TargetType
{
    Self,                   // 자신
    Target,                 // 스킬/코인의 현재 대상
    AllAllies,              // 아군 전체
    AllAlliesExceptSelf,    // 자신을 제외한 아군 전체
    AllEnemies,             // 적 전체
    RandomEnemy,            // 무작위 적 1명
    RandomAlly              // 무작위 아군 1명
}

public enum EffectTiming
{
    Passive,                // 상시 적용
    BeforeUse,              // 스킬 사용 직전
    OnUse,                  // 스킬 사용 시
    OnClashWin,             // 합 승리 시
    OnHit,                  // 적중 시
    OnBattleStart,          // 전투 시작 시
    OnTurnEnd,              // 턴 종료 시
}

public enum BuffType
{
    None,

    // 상태 이상 (디버프 및 버프)
    Bleed,                  // 출혈
    Burn,                   // 화상
    Tremor,                 // 진동
    Rupture,                // 파열
    Bind,                   // 속박
    Haste,                  // 신속

    // 인격 고유 키워드
    UniqueBreakthrough,     // 적진 주파
    UniqueConcussion,       // 뇌진탕
    UniqueDeathSignHaste,   // 주살 신속
    UniqueDeathSignRup,     // 주살 파
    UniqueDeathSignPoison,  // 주살 독
    UniqueLegPowerHorse,    // 각력
    UniqueCommand,          // 호령
}

public enum EffectType
{
    None,

    // 버프 관련 행동
    AddBuffPotency,         // 위력 증가
    AddBuffCount,           // 횟수 증가
    AddBuffPotencyNext,      // 다음 턴 버프 위력 증가
    AddBuffCountNext,       // 다음 턴 버프 횟수 증가
    ReduceBuffCount,        // 횟수 감소

    // 능력치 증감 및 전투 기믹
    CoinPowerUp,            // 코인 위력 증가
    FinalPowerUp,           // 최종 위력 증가
    DamageUpByBuff,         // 특정 버프 비례 피해량 증가
    DamageUpByBreakthrough, // 적진 주파 비례 피해량 증가
    DamageUpByShield,       // 보호막 수치 비례 피해량 증가
    DamageUp,               // 피해량 증가
    CoinPowerUpPerRupTrem,  // 파열+진동 합 비례 코인 위력 증가

    // 즉발성 기믹
    TremorBurst,            // 진동 폭발

    // 시스템 제어 및 특수 상태
    Indestructible,         // 파괴 불가 코인
    AllCoinsIndestructible, // 모든 코인을 파괴 불가로 변경
    TransformSkill,         // 스킬 변이
    SelfDispelNegative,     // 자신의 디버프 무작위 해제
}