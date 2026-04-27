using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [Header("설정")]
    public float baseSpeed = 5f;      // 물체의 기본 속도 (구름은 낮게, 장애물은 높게)
    public float destroyXPos = -15f;  // 삭제될 왼쪽 X 좌표 지점

    private PlayerController player;

    void Start()
    {
        // 씬에 있는 플레이어를 찾아서 참조합니다.
        player = GameObject.FindObjectOfType<PlayerController>();

        // 만약 플레이어를 찾지 못했을 때를 대비한 안전장치
        if (player == null)
        {
            Debug.LogError("씬에 PlayerController가 없습니다! 스크립트를 확인해주세요.");
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. 이동 로직: 플레이어의 속도 비율(0, 0.5, 1.0)에 따라 이동합니다.
        float moveDistance = baseSpeed * player.currentSpeedRatio * Time.deltaTime;
        transform.Translate(Vector2.left * moveDistance);

        // 2. 삭제 로직: 설정한 destroyXPos(예: -15)보다 왼쪽으로 가면 파괴합니다.
        if (transform.position.x < destroyXPos)
        {
            Destroy(gameObject);
        }
    }
}