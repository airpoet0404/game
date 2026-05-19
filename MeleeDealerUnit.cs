using System.Collections;
using UnityEngine;

// 체력 120 / 이동속도 1.5 / 공격력 50 / 광역(스플래시) / A키: 플레이어 위치 돌진 + 스플래시 공격
public class MeleeDealerUnit : UnitBase
{
    [Header("Melee Dealer Settings")]
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float splashRadius = 1.5f;

    private Transform playerTransform;
    private bool isSkillActive;

    protected override void Awake()
    {
        maxHP = 120f;
        moveSpeed = 1.5f;
        detectionRange = 6f;
        attackRange = 1.2f;
        attackCooldown = 1.5f;
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

        if (!IsInRange(currentTarget, attackRange))
        {
            state = UnitState.Chasing;
            MoveTo(currentTarget.position);
        }
        else
        {
            state = UnitState.Attacking;
            StopMove();
            TrySplashAttack(currentTarget.position);
        }
    }

    // A키 스킬 중: 플레이어 위치로 돌진 후 스플래시
    private void ExecuteSkillBehavior()
    {
        currentTarget = FindNearest(enemyLayer, detectionRange);

        if (currentTarget != null && IsInRange(currentTarget, attackRange))
        {
            StopMove();
            TrySplashAttack(currentTarget.position);
        }
        else
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer > 1.0f)
                MoveTo(playerTransform.position);
            else
                StopMove();
        }
    }

    // 공격 위치를 중심으로 splashRadius 내 모든 적에게 데미지
    private void TrySplashAttack(Vector2 center)
    {
        if (attackTimer > 0f) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, splashRadius, enemyLayer);
        int hitCount = 0;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyUnit>();
            if (enemy == null || enemy.IsDead) continue;
            enemy.TakeDamage(attackDamage);
            hitCount++;
        }

        attackTimer = attackCooldown;
        if (hitCount > 0)
            Debug.Log($"[MeleeDealer] 스플래시 공격: {hitCount}명 -{attackDamage} HP each");
    }

    // A키 스킬: 플레이어 위치로 즉시 돌진 + 3초간 집중 공격
    protected override void OnSkillActivated()
    {
        isSkillActive = true;
        StartCoroutine(SkillDurationRoutine(3f));
        Debug.Log("[MeleeDealer] A Skill: 플레이어 위치 돌진 + 스플래시 집중 공격!");
    }

    private IEnumerator SkillDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isSkillActive = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
