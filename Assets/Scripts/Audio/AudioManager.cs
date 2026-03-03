using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // Nguồn phát nhạc nền

    private void Awake()
    {
        // Khởi tạo Singleton để AudioManager không bị phá hủy khi qua Map mới
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm gọi để phát nhạc nền mới
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Nếu bài nhạc yêu cầu đang phát rồi thì bỏ qua để không bị phát lại từ đầu
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }
}