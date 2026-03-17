using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Điều khiển thanh máu Player trên HUD (góc màn hình).
/// Gắn vào PlayerHUD Canvas (Screen Space - Overlay).
/// Nối sự kiện: Health.OnHealthChanged → UpdateBar(int current, int max)
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo Slider thanh máu vào đây (tự tìm nếu để trống)")]
    public Slider healthSlider;

    [Header("Tuỳ chọn")]
    [Tooltip("Image fill — nếu gắn sẽ dùng thay cho Slider")]
    public Image fillImage;

    // --- Internal ---
    private float targetFill = 1f;
    private float currentFill = 1f;
    private float tweenSpeed = 5f;

    private void Awake()
    {
        // Tự tìm Slider nếu chưa gắn
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>(true);

        // Tự tìm Fill Image nếu chưa gắn (backup)
        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        // Khởi tạo đầy máu
        SetSliderValue(1f);
    }

    /// <summary>
    /// Gọi từ Health.OnHealthChanged(int current, int max).
    /// </summary>
    public void UpdateBar(int current, int max)
    {
        if (max <= 0) return;
        targetFill = Mathf.Clamp01((float)current / max);
        Debug.Log($"[PlayerHP] UpdateBar: {current}/{max} → fill={targetFill:F2}");
    }

    private void Update()
    {
        // Tween mượt
        if (!Mathf.Approximately(currentFill, targetFill))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, tweenSpeed * Time.deltaTime);
            SetSliderValue(currentFill);
        }
    }

    private void SetSliderValue(float value)
    {
        if (healthSlider != null)
            healthSlider.value = value;
        if (fillImage != null)
            fillImage.fillAmount = value;
    }
}
