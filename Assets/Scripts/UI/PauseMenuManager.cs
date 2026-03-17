using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;

    private bool isPaused = false;

    // THÊM MỚI: Biến chặn Slider tự động lưu đè dữ liệu bậy bạ lúc mới mở game
    private bool isInitialized = false;

    void Start()
    {
        // 1. Đọc dữ liệu từ bộ nhớ (Nếu người chơi chưa từng chơi, mặc định lấy 1f - Mức cao nhất)
        float savedBGMVolume = PlayerPrefs.GetFloat("BGMVolumePrefs", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolumePrefs", 1f);

        // 2. Gán giá trị cho các thanh trượt TRƯỚC KHI ẩn Menu
        if (bgmSlider != null) bgmSlider.value = savedBGMVolume;
        if (sfxSlider != null) sfxSlider.value = savedSFXVolume;

        // 3. Ép AudioMixer nhận mức âm lượng ngay lập tức
        SetVolume(savedBGMVolume);
        SetSFXVolume(savedSFXVolume);

        // 4. Ẩn Menu sau khi mọi thứ đã được setup xong xuôi
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // 5. Mở khóa cho phép lưu cài đặt từ nay về sau
        isInitialized = true;
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

    public void SetSFXVolume(float volume)
    {
        float dB = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
        mainMixer.SetFloat("SFXVolume", dB);

        // CẬP NHẬT: Chỉ lưu vào bộ nhớ nếu game đã khởi động xong
        if (isInitialized)
        {
            PlayerPrefs.SetFloat("SFXVolumePrefs", volume);
            PlayerPrefs.Save();
        }
    }

    // Hàm này sẽ được gọi mỗi khi kéo thanh Slider (Nhạc nền)
    public void SetVolume(float sliderValue)
    {
        float volumeInDb = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        mainMixer.SetFloat("BGMVolume", volumeInDb);

        // CẬP NHẬT: Chỉ lưu vào bộ nhớ nếu game đã khởi động xong
        if (isInitialized)
        {
            PlayerPrefs.SetFloat("BGMVolumePrefs", sliderValue);
            PlayerPrefs.Save();
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetToDefault()
    {
        // Kéo slider và set BGM về mức cao nhất (1f tương đương 0 dB)
        if (bgmSlider != null) bgmSlider.value = 1f;
        SetVolume(1f);

        // Kéo slider và set SFX về mức cao nhất (1f tương đương 0 dB)
        if (sfxSlider != null) sfxSlider.value = 1f;
        SetSFXVolume(1f);
    }
}