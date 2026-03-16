using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RestartLevel()
    {
        // Lấy tên scene đã lưu từ GameManager hoặc scene hiện tại trước khi chết
        // Tạm thời reload scene active cuối cùng (nếu GameManager chưa setup)
        string lastScene = PlayerPrefs.GetString("LastLevel", "Map1");
        SceneManager.LoadScene(lastScene);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
