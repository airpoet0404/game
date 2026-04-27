using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("속도 설정")]
    public float currentSpeedRatio = 0f; 
    private float shiftPressedTime = 0f;

    [Header("점프 설정")]
    public float jumpForce = 12f;       // 점프 힘 (조금 더 높였습니다)
    private bool isGrounded = true;    
    private Rigidbody2D rb;

    [Header("컴포넌트 참조")]
    private Animator anim;             // 애니메이터 추가
    private GameManager gameManager; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // 애니메이터 가져오기
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        // 게임 오버 상태라면 애니메이션도 정지하고 조작 막기
        if (gameManager != null && gameManager.isGameOver) 
        {
            anim.SetFloat("Speed", 0);
            return;
        }

        HandleMovement(); // 이동/속도 관리
        HandleJump();     // 점프 관리

        // --- 애니메이터 파라미터 업데이트 ---
        // 1. Speed 전달 (0, 0.5, 1.0 중 하나가 들어감)
        anim.SetFloat("Speed", currentSpeedRatio);
        
        // 2. 바닥 여부 전달 (공중에 있으면 false)
        anim.SetBool("isGrounded", isGrounded);
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            shiftPressedTime += Time.deltaTime;
            currentSpeedRatio = (shiftPressedTime >= 3f) ? 1.0f : 0.5f;
        }
        else
        {
            shiftPressedTime = 0f;
            currentSpeedRatio = 0f;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 속도(Velocity)를 직접 수정하여 즉각적인 점프 구현
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 바닥 태그가 "Ground"인 물체와 닿으면 착지 판정
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void Die()
    {
        currentSpeedRatio = 0; 
        anim.SetFloat("Speed", 0); // 죽을 때 애니메이션 멈춤
        
        GetComponent<SpriteRenderer>().color = Color.red;

        if (gameManager != null)
        {
            gameManager.EndGame();
        }
    }
}