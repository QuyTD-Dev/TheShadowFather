using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Bắt buộc phải có để dùng Input System mới

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuPanel;
    public Slider bgmSlider;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;

    private bool isPaused = false;

    void Start()
    {
        // Đảm bảo menu ẩn khi bắt đầu game
        pauseMenuPanel.SetActive(false);

        // Cập nhật giá trị slider khớp với âm lượng hiện tại (Mặc định là 1)
        bgmSlider.value = 1f;
    }

    void Update()
    {
        // Bắt sự kiện nhấn phím ESC bằng New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Dừng thời gian trong game
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // Chạy lại thời gian
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Phải set lại = 1 nếu không scene mới sẽ bị đứng hình
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Hàm này sẽ được gọi mỗi khi kéo thanh Slider
    public void SetVolume(float sliderValue)
    {
        // Chuyển đổi giá trị Linear (0.0001 -> 1) sang Logarithmic (-80dB -> 0dB)
        float volumeInDb = Mathf.Log10(sliderValue) * 20f;
        mainMixer.SetFloat("BGMVolume", volumeInDb);
    }

    public void ResetToDefault()
    {
        bgmSlider.value = 1f; // Kéo slider về max
        SetVolume(1f);        // Set âm lượng về max
    }
}