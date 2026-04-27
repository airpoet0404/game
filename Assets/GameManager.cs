using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool isGameOver = false;

    public void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("게임 오버! 2초 후 재시작합니다.");

        // 시간을 느리게 하거나 멈춰서 게임오버 느낌을 줍니다.
        Time.timeScale = 0.5f; 

        // 2초 뒤에 Restart 함수를 실행합니다.
        Invoke("Restart", 2f);
    }

    void Restart()
    {
        Time.timeScale = 1f; // 시간 속도를 정상으로 돌림
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}