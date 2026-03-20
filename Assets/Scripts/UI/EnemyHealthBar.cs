using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển thanh máu nổi trên đầu quái (World Space Canvas).
/// Gắn vào Canvas child của quái. Canvas phải là World Space.
/// Nối sự kiện: Health.OnHealthChanged → UpdateBar(int current, int max)
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image fill của thanh máu chính (màu vàng/xanh)")]
    public Image fillImage;

    [Tooltip("Image fill của delay bar (màu tối hơn, chạy chậm)")]
    public Image delayImage;

    [Header("Animation")]
    public float fillSpeed = 6f;
    public float delaySpeed = 2f;

    [Header("Hiển thị")]
    [Tooltip("Ẩn thanh máu khi quái đầy máu")]
    public bool hideWhenFull = true;

    [Tooltip("Tự ẩn sau X giây không bị đánh (0 = không tự ẩn)")]
    public float autoHideDelay = 3f;

    [Header("Billboard")]
    [Tooltip("Luôn xoay về Camera (để thanh máu không bị lật khi quái flip)")]
    public bool useBillboard = true;

    // --- Internal ---
    private float targetFill = 1f;
    private float currentFill = 1f;
    private float currentDelay = 1f;
    private float hideTimer = 0f;
    private bool isVisible = false;
    private Canvas canvas;
    private Camera mainCamera;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;

        // Ẩn ban đầu nếu cần
        //if (hideWhenFull && canvas != null)
        //    canvas.enabled = false;
    }

    private void LateUpdate()
    {
        // Billboard: xoay Canvas nhìn về Camera
        if (useBillboard && mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }

        // --- Tween thanh máu chính ---
        if (!Mathf.Approximately(currentFill, targetFill))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.deltaTime);
            if (fillImage != null)
                fillImage.fillAmount = currentFill;
        }

        // --- Tween delay bar ---
        if (currentDelay > currentFill)
        {
            currentDelay = Mathf.MoveTowards(currentDelay, currentFill, delaySpeed * Time.deltaTime);
            if (delayImage != null)
                delayImage.fillAmount = currentDelay;
        }
        else
        {
            currentDelay = currentFill;
            if (delayImage != null)
                delayImage.fillAmount = currentDelay;
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

        targetFill = Mathf.Clamp01((float)current / max);

        bool shouldShow = current < max && current > 0;

        if (shouldShow)
        {
            SetVisible(true);
            // Reset bộ đếm auto-hide
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
