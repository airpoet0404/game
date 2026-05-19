using UnityEngine;

// 체력 300 / 이동속도 1.0 / 공격 불가 / D키: 가장 가까운 적 스킬 봉쇄
public class TankerUnit : UnitBase
{
    [Header("Tanker Skill")]
    [SerializeField] private float skillSealDuration = 3f;
    [SerializeField] private float skillSealRange = 5f;

    protected override void Awake()
    {
        maxHP = 300f;
        moveSpeed = 1.0f;
        detectionRange = 6f;
        attackRange = 1.5f;
        base.Awake();
    }

    // 탱커 AI: 가장 가까운 적에게 접근해 선점, 공격하지 않음
    protected override void UpdateBehavior()
    {
        currentTarget = FindNearest(enemyLayer, detectionRange);

        if (currentTarget == null)
        {
            state = UnitState.Idle;
            StopMove();
            return;
        }

        if (!IsInRange(currentTarget, attackRange))
        {
            state = UnitState.Chasing;
            MoveTo(currentTarget.position);
        }
        else
        {
            // 적과 대치 상태 유지 (공격 없음)
            state = UnitState.Idle;
            StopMove();
        }
    }

    // D키 스킬: 가장 가까운 적의 스킬 사용 봉쇄
    protected override void OnSkillActivated()
    {
        Transform nearest = FindNearest(enemyLayer, skillSealRange);
        if (nearest == null)
        {
            Debug.Log("[Tanker] D Skill: 범위 내 봉쇄할 적 없음");
            return;
        }

        var enemy = nearest.GetComponent<EnemyUnit>();
        if (enemy != null)
            enemy.ApplySkillSeal(skillSealDuration);

        Debug.Log($"[Tanker] D Skill: {nearest.name} 스킬 봉쇄 {skillSealDuration}초");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, skillSealRange);
    }
}
