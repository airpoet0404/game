using System.Collections;
using UnityEngine;

// 체력 120 / 이동속도 1.5 / 공격력 50 / 필중 단일 타겟 / A키: 플레이어 위치 집결 + 집중 공격
public class RangedDealerUnit : UnitBase
{
    [Header("Ranged Dealer Settings")]
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float keepDistance = 4f;   // 유지하려는 사거리
    [SerializeField] private float retreatDistance = 2f; // 후퇴 기준 거리

    private Transform playerTransform;
    private bool isSkillActive;

    protected override void Awake()
    {
        maxHP = 120f;
        moveSpeed = 1.5f;
        detectionRange = 8f;
        attackRange = 5f;
        attackCooldown = 1.2f;
        base.Awake();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    protected override void UpdateBehavior()
    {
        if (isSkillActive && playerTransform != null)
        {
            ExecuteSkillBehavior();
            return;
        }

        currentTarget = FindNearest(enemyLayer, detectionRange);

        if (currentTarget == null)
        {
            state = UnitState.Idle;
            StopMove();
            return;
        }

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist < retreatDistance)
        {
            // 적이 너무 가까우면 후퇴 (카이팅 방지는 공격 측이 아닌 이 유닛 자신을 위한 포지셔닝)
            state = UnitState.Chasing;
            Vector2 retreatDir = (transform.position - currentTarget.position).normalized;
            rb.linearVelocity = retreatDir * moveSpeed;
        }
        else if (dist > attackRange)
        {
            state = UnitState.Chasing;
            MoveTo(currentTarget.position);
        }
        else
        {
            state = UnitState.Attacking;
            StopMove();
            TryAttack(currentTarget);
        }
    }

    // A키 스킬 중: 플레이어 주변으로 이동 후 집중 공격
    private void ExecuteSkillBehavior()
    {
        currentTarget = FindNearest(enemyLayer, detectionRange);

        if (currentTarget != null && IsInRange(currentTarget, attackRange))
        {
            StopMove();
            TryAttack(currentTarget);
        }
        else
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer > 1.5f)
                MoveTo(playerTransform.position);
            else
                StopMove();
        }
    }

    // 필중 판정: 거리나 이동과 무관하게 즉시 데미지 적용
    private void TryAttack(Transform target)
    {
        if (attackTimer > 0f || target == null) return;

        var enemy = target.GetComponent<EnemyUnit>();
        if (enemy == null || enemy.IsDead) return;

        enemy.TakeDamage(attackDamage); // 필중 - 투사체 없이 즉시 적용
        attackTimer = attackCooldown;
        Debug.Log($"[RangedDealer] 필중 공격: {target.name} -{attackDamage} HP");
    }

    // A키 스킬: 플레이어 위치 집결 + 집중 공격 (3초)
    protected override void OnSkillActivated()
    {
        isSkillActive = true;
        StartCoroutine(SkillDurationRoutine(3f));
        Debug.Log("[RangedDealer] A Skill: 플레이어 위치 집결 + 집중 공격!");
    }

    private IEnumerator SkillDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isSkillActive = false;
    }
}
