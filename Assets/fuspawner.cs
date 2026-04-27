using UnityEngine;

public class fuspawner : MonoBehaviour // 파일 이름이 fuspawner.cs라면 이대로 두세요!
{
    [Header("생성 설정")]
    public GameObject prefab;      
    public float interval = 2f;    
    
    [Header("위치 설정")]
    public bool isRandomY = false; 
    public float minY = 1f;        
    public float maxY = 4f;        
    public float fixedY = -3.5f;   
    public float spawnX = 15f;     

    private float timer;
    private PlayerController player;

    void Start()
    {
        player = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player == null) return;

        if (player.currentSpeedRatio > 0)
        {
            timer += Time.deltaTime * player.currentSpeedRatio;

            if (timer >= interval)
            {
                // 함수 이름을 명확하게 CreatePrefab으로 호출합니다.
                CreatePrefab(); 
                timer = 0;
            }
        }
    }

    // 이름을 아예 바꿔서 혼동을 피합니다.
    void CreatePrefab() 
    {
        float yPos = isRandomY ? Random.Range(minY, maxY) : fixedY;
        Vector3 spawnPos = new Vector3(spawnX, yPos, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}