using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    [Header("Tên Scene muốn chuyển tới")]
    public string nextSceneName = "IntroVideo";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng chạm vào cổng có phải là Player không
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Nhân vật đã vào cổng! Chuyển sang Intro...");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}