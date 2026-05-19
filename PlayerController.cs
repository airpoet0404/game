using UnityEngine;

// 플레이어 컨트롤러 - 방향키/WASD 이동, 힐러 힐 대상 포함
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float maxHP = 200f;
    [SerializeField] private float moveSpeed = 2.5f;

    public float CurrentHP { get; private set; }
    public float MaxHP => maxHP;
    public bool IsDead { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
        // 힐러가 FindWithTag("Player")로 찾을 수 있도록 태그 설정
        // Inspector에서 태그를 "Player"로 지정하거나 아래 줄 사용
        // gameObject.tag = "Player";
    }

    private void Update()
    {
        if (IsDead) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        rb.linearVelocity = dir * moveSpeed;

        if (h != 0f) spriteRenderer.flipX = h < 0f;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - damage);
        Debug.Log($"[Player] 피격 -{damage} HP | 현재 HP: {CurrentHP}/{maxHP}");
        if (CurrentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, maxHP);
        Debug.Log($"[Player] 힐 +{amount} HP | 현재 HP: {CurrentHP}/{maxHP}");
    }

    private void Die()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("[Player] 사망!");
    }
}
