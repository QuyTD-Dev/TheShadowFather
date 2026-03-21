using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public void ReplayGame()
    {
        Time.timeScale = 1f; // Trả lại thời gian bình thường
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Tải lại cảnh hiện tại
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Hãy thay "MainMenu" bằng tên Scene Menu thực tế của bạn
        SceneManager.LoadScene("MainMenu");
    }
}