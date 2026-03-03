using UnityEngine;

public class MapMusicPlayer : MonoBehaviour
{
    [Header("BGM cho Map này")]
    public AudioClip mapBGM;

    private void Start()
    {
        // Báo cho AudioManager phát bài nhạc được gán
        if (AudioManager.Instance != null && mapBGM != null)
        {
            AudioManager.Instance.PlayMusic(mapBGM);
        }
    }
}