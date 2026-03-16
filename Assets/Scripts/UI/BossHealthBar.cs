using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Điều khiển thanh máu Boss nổi trên đầu boss (World Space Canvas).
/// Kích thước to và dài hơn thanh quái thường.
/// Gắn vào Canvas child của Boss GameObject.
/// Nối sự kiện: BossHealth.OnHealthChanged → UpdateBar(int current, int max)
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image fill thanh máu chính (màu đỏ tươi)")]
    public Image fillImage;

    [Tooltip("Image fill delay bar (màu đỏ tối)")]
    public Image delayImage;

    [Tooltip("Text tên Boss (ví dụ: 'SHADOW BEAR')")]
    public TextMeshProUGUI bossNameText;

    [Tooltip("Text pha hiện tại (ví dụ: 'PHASE 1')")]
    public TextMeshProUGUI phaseText;

    [Header("Cấu hình Boss")]
    [Tooltip("Tên hiển thị của Boss")]
    public string bossName = "SHADOW BEAR";

    [Header("Animation")]
    public float fillSpeed = 5f;
    public float delaySpeed = 1.8f;

    [Header("Billboard")]
    [Tooltip("Luôn xoay nhìn về Camera")]
    public bool useBillboard = true;

    [Header("Hiệu ứng Phase Transition")]
    [Tooltip("Màu fill bar ở Phase 1")]
    public Color phase1Color = new Color(0.9f, 0.15f, 0.1f);
    [Tooltip("Màu fill bar ở Phase 2 (đỏ sậm/cam)")]
    public Color phase2Color = new Color(1f, 0.4f, 0f);

    // --- Internal ---
    private float targetFill = 1f;
    private float currentFill = 1f;
    private float currentDelay = 1f;
    private int currentPhase = 1;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (bossNameText != null)
            bossNameText.text = bossName;

        SetPhaseText(1);

        if (fillImage != null)
            fillImage.color = phase1Color;
    }

    private void LateUpdate()
    {
        // Billboard
        if (useBillboard && mainCamera != null)
            transform.rotation = mainCamera.transform.rotation;

        // Tween thanh máu chính
        if (!Mathf.Approximately(currentFill, targetFill))
        {
            currentFill = Mathf.MoveTowards(currentFill, targetFill, fillSpeed * Time.deltaTime);
            if (fillImage != null)
                fillImage.fillAmount = currentFill;
        }

        // Tween delay bar
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
    }

    /// <summary>
    /// Gọi từ BossHealth.OnHealthChanged(int current, int max).
    /// Script BossHealth tự tính max theo phase hiện tại.
    /// </summary>
    public void UpdateBar(int current, int max)
    {
        if (max <= 0) return;
        targetFill = Mathf.Clamp01((float)current / max);
    }

    /// <summary>
    /// Gọi khi boss chuyển sang Phase 2.
    /// Thay đổi màu thanh máu và text phase.
    /// </summary>
    public void OnPhase2Start()
    {
        currentPhase = 2;
        SetPhaseText(2);

        // Thanh máu đầy lại (phase 2 hồi máu)
        targetFill = 1f;
        currentFill = 1f;
        currentDelay = 1f;
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = phase2Color;
        }
        if (delayImage != null)
            delayImage.fillAmount = 1f;
    }

    private void SetPhaseText(int phase)
    {
        if (phaseText != null)
            phaseText.text = $"PHASE {phase}";
    }
}
