using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Điều khiển thanh máu Player trên HUD (góc màn hình).
/// Gắn vào HealthBarPanel trên Canvas PlayerHUD (Screen Space - Overlay).
/// Nối sự kiện: Health.OnHealthChanged → UpdateBar(int current, int max)
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image fill của thanh máu chính (màu đỏ tươi)")]
    public Image fillImage;

    [Tooltip("Image fill của delay bar (màu đỏ tối, chạy chậm hơn)")]
    public Image delayImage;

    [Tooltip("(Tuỳ chọn) Text hiển thị '75 / 100'")]
    public TextMeshProUGUI hpText;

    [Header("Animation")]
    [Tooltip("Tốc độ tween thanh máu chính (đơn vị/giây)")]
    public float fillSpeed = 5f;

    [Tooltip("Tốc độ tween delay bar (chậm hơn để tạo hiệu ứng máu chảy)")]
    public float delaySpeed = 1.5f;

    [Header("Màu (tuỳ chọn)")]
    public bool useColorGradient = true;
    public Gradient healthGradient;   // Xanh (đầy) → Vàng → Đỏ (cạn)

    // --- Internal ---
    private float targetFill;       // Giá trị fill đích (0-1)
    private float currentFill;      // Giá trị fill hiện tại (để tween)
    private float currentDelay;     // Giá trị delay bar hiện tại
    private int cachedMax = 1;

    private void Awake()
    {
        // Mặc định đầy máu
        targetFill = 1f;
        currentFill = 1f;
        currentDelay = 1f;

        if (fillImage != null) fillImage.fillAmount = 1f;
        if (delayImage != null) delayImage.fillAmount = 1f;
    }

    /// <summary>
    /// Gọi từ Health.OnHealthChanged(int current, int max).
    /// </summary>
    public void UpdateBar(int current, int max)
    {
        if (max <= 0) return;
        cachedMax = max;
        targetFill = Mathf.Clamp01((float)current / max);

        if (hpText != null)
            hpText.text = $"{Mathf.Max(current, 0)} / {max}";
    }

    private void Update()
    {
        // --- Tween thanh máu chính ---
        if (!Mathf.Approximately(currentFill, targetFill))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.deltaTime);
            if (fillImage != null)
                fillImage.fillAmount = currentFill;

            // Cập nhật màu
            if (useColorGradient && fillImage != null)
                fillImage.color = healthGradient.Evaluate(currentFill);
        }

        // --- Tween delay bar (chỉ giảm, không tăng) ---
        if (currentDelay > currentFill)
        {
            // Đợi một chút rồi mới bắt đầu kéo delay bar xuống
            currentDelay = Mathf.MoveTowards(currentDelay, currentFill, delaySpeed * Time.deltaTime);
            if (delayImage != null)
                delayImage.fillAmount = currentDelay;
        }
        else
        {
            // Khi hồi máu: delay bar theo kịp ngay
            currentDelay = currentFill;
            if (delayImage != null)
                delayImage.fillAmount = currentDelay;
        }
    }
}
