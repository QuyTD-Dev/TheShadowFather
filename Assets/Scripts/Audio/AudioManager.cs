using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // Nguồn phát nhạc nền
    [SerializeField] private AudioSource sfxSource; // Thêm: Nguồn phát hiệu ứng âm thanh (SFX)

    private void Awake()
    {
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

    // Hàm phát nhạc nền (giữ nguyên)
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // THÊM MỚI: Hàm phát hiệu ứng âm thanh (SFX)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Dùng PlayOneShot để các âm thanh SFX (như tiếng chém liên tục) có thể đè lên nhau mà không bị ngắt
        sfxSource.PlayOneShot(clip, volume);
    }
}