using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tên của cảnh (Scene) muốn chuyển đến sau khi xem xong video.")]
    public string nextSceneName = "SampleScene";

    [Tooltip("Phím dùng để bỏ qua video.")]
    public KeyCode skipKey = KeyCode.Space;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Start()
    {
        if (videoPlayer != null)
        {
            // Đăng ký sự kiện khi video kết thúc
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("Không tìm thấy component VideoPlayer trên GameObject này!");
        }
    }

    private void Update()
    {
        // Nhấn phím để bỏ qua video
        if (Input.GetKeyDown(skipKey))
        {
            LoadNextScene();
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Chưa thiết lập Next Scene Name trong VideoController!");
        }
    }
}
