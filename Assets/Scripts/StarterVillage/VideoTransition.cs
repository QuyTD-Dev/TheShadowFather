using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video; // Thư viện bắt buộc để thao tác với Video

public class VideoTransition : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    [Header("Tên Scene tiếp theo (Map 1)")]
    public string nextSceneName = "Map1"; // Sửa lại thành tên Scene Map 1 của bạn

    void Start()
    {
        // Lấy component VideoPlayer nếu chưa được kéo vào
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Lệnh này báo cho Unity biết: Khi video chạy đến điểm cuối cùng, hãy gọi hàm EndReached
        videoPlayer.loopPointReached += EndReached;
    }

    // Hàm này sẽ tự động chạy khi video kết thúc
    void EndReached(VideoPlayer vp)
    {
        Debug.Log("Video đã phát xong! Tiến vào Map chính...");
        SceneManager.LoadScene(nextSceneName);
    }
}