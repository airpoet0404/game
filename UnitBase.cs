using System.Collections;
using UnityEngine;

public enum UnitState { Idle, Chasing, Attacking, Healing, Skill }

public abstract class UnitBase : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHP = 100f;
    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected float detectionRange = 6f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1.5f;

    public float CurrentHP { get; protected set; }
    public float MaxHP => maxHP;
    public bool IsDead { get; protected set; }
    public bool IsInvincible { get; private set; }
    public bool IsSkillSealed { get; private set; }

    [Header("Layers")]
    public LayerMask enemyLayer;
    public LayerMask allyLayer;

    protected UnitState state = UnitState.Idle;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Transform currentTarget;
    protected float attackTimer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
    }

    protected virtual void Update()
    {
        if (IsDead) return;
        attackTimer -= Time.deltaTime;
        UpdateBehavior();
    }

    protected abstract void UpdateBehavior();
    protected abstract void OnSkillActivated();

    public virtual void TakeDamage(float damage)
    {
        if (IsInvincible || IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);
        if (CurrentHP <= 0f) Die();
    }

    public virtual void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
    }

    protected virtual void Die()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    public void TryUseSkill()
    {
        if (IsDead || IsSkillSealed) return;
        OnSkillActivated();
    }

    public void ApplyInvincible(float duration) => StartCoroutine(InvincibleRoutine(duration));
    public void ApplySkillSeal(float duration) => StartCoroutine(SkillSealRoutine(duration));

    private IEnumerator InvincibleRoutine(float duration)
    {
        IsInvincible = true;
        spriteRenderer.color = new Color(1f, 1f, 0f, 0.7f); // 황금색 피드백
        yield return new WaitForSeconds(duration);
        IsInvincible = false;
        spriteRenderer.color = Color.white;
    }

    private IEnumerator SkillSealRoutine(float duration)
    {
        IsSkillSealed = true;
        yield return new WaitForSeconds(duration);
        IsSkillSealed = false;
    }

    protected void MoveTo(Vector3 pos)
    {
        Vector2 dir = (pos - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (dir.x != 0f) spriteRenderer.flipX = dir.x < 0f;
    }

    protected void StopMove() => rb.linearVelocity = Vector2.zero;

    protected bool IsInRange(Transform t, float range)
        => t != null && Vector2.Distance(transform.position, t.position) <= range;

    protected Transform FindNearest(LayerMask layer, float range)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, layer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            var unit = hit.GetComponent<UnitBase>();
            if (unit != null && unit.IsDead) continue;
            var enemy = hit.GetComponent<EnemyUnit>();
            if (enemy != null && enemy.IsDead) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minDist) { minDist = d; nearest = hit.transform; }
        }
        return nearest;
    }

    // allyLayer 내에서 HP 비율이 가장 낮은 아군 탐색
    protected Transform FindLowestHPAlly(float range)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, allyLayer);
        Transform lowest = null;
        float lowestRatio = float.MaxValue;

        foreach (var hit in hits)
        {
            var unit = hit.GetComponent<UnitBase>();
            if (unit == null || unit.IsDead) continue;
            float ratio = unit.CurrentHP / unit.MaxHP;
            if (ratio < 1f && ratio < lowestRatio) { lowestRatio = ratio; lowest = hit.transform; }
        }
        return lowest;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
