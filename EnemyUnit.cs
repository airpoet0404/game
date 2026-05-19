using System.Collections;
using UnityEngine;

// 기본 적 유닛 - 가장 가까운 아군/플레이어를 추적하여 공격
// 탱커의 D키 스킬에 의해 스킬 봉쇄 가능
public class EnemyUnit : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHP = 150f;
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float detectionRange = 7f;

    [Header("Skill")]
    [SerializeField] private float skillCooldown = 8f;
    [SerializeField] private float skillDamage = 30f;
    [SerializeField] private float skillRange = 3f;

    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public bool IsDead { get; private set; }
    public bool IsSkillSealed { get; private set; }

    [Header("Layers")]
    public LayerMask targetLayer; // Ally + Player 레이어

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform currentTarget;
    private float attackTimer;
    private float skillTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
    }

    private void Update()
    {
        if (IsDead) return;
        attackTimer -= Time.deltaTime;
        skillTimer -= Time.deltaTime;
        UpdateAI();
    }

    private void UpdateAI()
    {
        currentTarget = FindNearestTarget();

        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, currentTarget.position);

        if (dist > attackRange)
        {
            Vector2 dir = (currentTarget.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
            if (dir.x != 0f) spriteRenderer.flipX = dir.x < 0f;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TrySkill();  // 스킬 우선 시도
            TryAttack();
        }
    }

    private Transform FindNearestTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            // 죽은 유닛 제외
            var unit = hit.GetComponent<UnitBase>();
            if (unit != null && unit.IsDead) continue;
            var player = hit.GetComponent<PlayerController>();
            if (player != null && player.IsDead) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; nearest = hit.transform; }
        }
        return nearest;
    }

    private void TryAttack()
    {
        if (attackTimer > 0f || currentTarget == null) return;

        ApplyDamageToTarget(currentTarget, attackDamage);
        attackTimer = attackCooldown;
    }

    // 적 스킬: 범위 광역 공격 (봉쇄 대상)
    private void TrySkill()
    {
        if (IsSkillSealed || skillTimer > 0f) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, skillRange, targetLayer);
        foreach (var hit in hits)
            ApplyDamageToTarget(hit.transform, skillDamage);

        skillTimer = skillCooldown;
        Debug.Log($"[Enemy] {name} 스킬 사용: 광역 -{skillDamage} HP");
    }

    private void ApplyDamageToTarget(Transform target, float damage)
    {
        var unit = target.GetComponent<UnitBase>();
        if (unit != null && !unit.IsDead) { unit.TakeDamage(damage); return; }

        var player = target.GetComponent<PlayerController>();
        if (player != null && !player.IsDead) player.TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);
        if (CurrentHP <= 0f) Die();
    }

    // 탱커 D키 스킬에 의해 호출
    public void ApplySkillSeal(float duration) => StartCoroutine(SkillSealRoutine(duration));

    private IEnumerator SkillSealRoutine(float duration)
    {
        IsSkillSealed = true;
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f); // 봉쇄 시각 피드백
        yield return new WaitForSeconds(duration);
        IsSkillSealed = false;
        spriteRenderer.color = Color.white;
    }

    private void Die()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, skillRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
