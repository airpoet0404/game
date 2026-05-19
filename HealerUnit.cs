using System.Collections;
using UnityEngine;

// 체력 80 / 이동속도 1.3 / 힐량 20 / S키: 범위 내 아군+플레이어 즉시 회복
public class HealerUnit : UnitBase
{
    [Header("Healer Settings")]
    [SerializeField] private float healAmount = 20f;
    [SerializeField] private float healCooldown = 2f;
    [SerializeField] private float healRange = 4f;
    [SerializeField] private float burstHealAmount = 60f;
    [SerializeField] private float burstHealRange = 5f;

    private float healTimer;
    private Transform playerTransform;

    protected override void Awake()
    {
        maxHP = 80f;
        moveSpeed = 1.3f;
        detectionRange = 6f;
        attackRange = healRange;
        base.Awake();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    protected override void Update()
    {
        healTimer -= Time.deltaTime;
        base.Update();
    }

    protected override void UpdateBehavior()
    {
        Transform healTarget = FindHealTarget();

        if (healTarget == null)
        {
            // 힐 대상 없으면 플레이어 주변 대기
            if (playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist > 2.5f) MoveTo(playerTransform.position);
                else StopMove();
            }
            else
            {
                state = UnitState.Idle;
                StopMove();
            }
            return;
        }

        if (!IsInRange(healTarget, attackRange))
        {
            state = UnitState.Chasing;
            MoveTo(healTarget.position);
        }
        else
        {
            state = UnitState.Healing;
            StopMove();
            TryHeal(healTarget);
        }
    }

    // HP 비율이 가장 낮은 대상 탐색 (아군 유닛 + 플레이어 포함)
    private Transform FindHealTarget()
    {
        Transform lowestAlly = FindLowestHPAlly(healRange);
        float allyRatio = lowestAlly != null
            ? lowestAlly.GetComponent<UnitBase>().CurrentHP / lowestAlly.GetComponent<UnitBase>().MaxHP
            : 1f;

        // 플레이어도 힐 대상에 포함
        if (playerTransform != null)
        {
            var playerCtrl = playerTransform.GetComponent<PlayerController>();
            if (playerCtrl != null && !playerCtrl.IsDead)
            {
                float playerDist = Vector2.Distance(transform.position, playerTransform.position);
                float playerRatio = playerCtrl.CurrentHP / playerCtrl.MaxHP;

                if (playerDist <= healRange && playerRatio < allyRatio && playerRatio < 1f)
                    return playerTransform;
            }
        }

        return lowestAlly;
    }

    private void TryHeal(Transform target)
    {
        if (healTimer > 0f || target == null) return;

        var unit = target.GetComponent<UnitBase>();
        if (unit != null && !unit.IsDead)
        {
            unit.Heal(healAmount);
            healTimer = healCooldown;
            Debug.Log($"[Healer] {target.name} 힐 +{healAmount} HP");
            return;
        }

        var player = target.GetComponent<PlayerController>();
        if (player != null && !player.IsDead)
        {
            player.Heal(healAmount);
            healTimer = healCooldown;
            Debug.Log($"[Healer] Player 힐 +{healAmount} HP");
        }
    }

    // S키 스킬: 범위 내 모든 아군 + 플레이어 즉시 버스트 힐
    protected override void OnSkillActivated()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, burstHealRange, allyLayer);
        int healCount = 0;

        foreach (var hit in hits)
        {
            var unit = hit.GetComponent<UnitBase>();
            if (unit == null || unit.IsDead) continue;
            unit.Heal(burstHealAmount);
            healCount++;
        }

        // 플레이어 힐
        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer <= burstHealRange)
            {
                var playerCtrl = playerTransform.GetComponent<PlayerController>();
                if (playerCtrl != null && !playerCtrl.IsDead)
                {
                    playerCtrl.Heal(burstHealAmount);
                    healCount++;
                }
            }
        }

        Debug.Log($"[Healer] S Skill: 버스트 힐 +{burstHealAmount} HP × {healCount}명");
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, burstHealRange);
    }
}
