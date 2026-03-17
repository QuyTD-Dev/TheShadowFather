using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển thanh máu nổi trên đầu quái (World Space Canvas).
/// Gắn vào root object của EnemyHealthBar prefab.
/// Nối sự kiện: Health.OnHealthChanged → UpdateBar(int current, int max)
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo Slider thanh máu vào đây")]
    public Slider healthSlider;

    [Header("Hiển thị")]
    [Tooltip("Ẩn thanh máu khi quái đầy máu")]
    public bool hideWhenFull = true;

    [Tooltip("Tự ẩn sau X giây không bị đánh (0 = không tự ẩn)")]
    public float autoHideDelay = 3f;

    [Header("Billboard")]
    [Tooltip("Luôn xoay về Camera (để thanh máu không bị lật khi quái flip)")]
    public bool useBillboard = true;

    // --- Internal ---
    private float hideTimer = 0f;
    private bool isVisible = false;
    private Canvas canvas;
    private Camera mainCamera;

    private void Awake()
    {
        // Tìm Canvas trong children (vì Canvas là con của root EnemyHealthBar object)
        canvas = GetComponentInChildren<Canvas>(true);
        mainCamera = Camera.main;

        // Tự tìm Slider nếu chưa gắn
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);

        // Ẩn ban đầu nếu cần
        if (hideWhenFull && canvas != null)
            canvas.enabled = false;
    }

    private void LateUpdate()
    {
        // Billboard: xoay Canvas nhìn về Camera
        if (useBillboard && mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        // --- Auto-hide countdown ---
        if (isVisible && autoHideDelay > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                SetVisible(false);
            }
        }
    }

    /// <summary>
    /// Gọi từ Health.OnHealthChanged(int current, int max).
    /// </summary>
    public void UpdateBar(int current, int max)
    {
        if (max <= 0) return;

        float fill = Mathf.Clamp01((float)current / max);

        // Cập nhật Slider
        if (healthSlider != null)
        {
            healthSlider.value = fill;
        }

        bool shouldShow = current < max && current > 0;

        if (shouldShow)
        {
            SetVisible(true);
            hideTimer = autoHideDelay;
        }
        else if (current <= 0)
        {
            // Quái chết: ẩn luôn
            SetVisible(false);
        }
        else if (hideWhenFull)
        {
            // Đầy máu: ẩn
            SetVisible(false);
        }
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (canvas != null)
            canvas.enabled = visible;
    }
}
