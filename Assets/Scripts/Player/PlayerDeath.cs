using UnityEngine;
using System.Collections;
using TheShadowFather.Player;

/// <summary>
/// Xử lý logic chết của Player:
/// - Kích hoạt animation Die
/// - Tắt PlayerController (không điều khiển được nữa)
/// - Tắt Rigidbody để Player đứng yên
/// Gắn vào Player prefab, nối với Health.OnDied event.
/// </summary>
public class PlayerDeath : MonoBehaviour
{
    [Header("Cấu hình")]
    [Tooltip("Thời gian chờ sau animation Die trước khi tắt Player")]
    public float disableDelay = 2f;

    // Animator hash (tối ưu performance)
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Animator animator;
    private PlayerController controller;
    private Rigidbody2D rb;
    private bool isDead = false;

    private void Awake()
    {
        animator   = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rb         = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Gọi hàm này từ Health.OnDied UnityEvent trong Inspector.
    /// </summary>
    public void OnPlayerDied()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerDeath] Player đã hết máu!");

        // 1. Tắt PlayerController để dừng input
        if (controller != null) controller.enabled = false;

        // 2. Dừng chuyển động vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Không bị vật lý tác động nữa
        }

        // 3. Trigger animation Die
        if (animator != null)
        {
            animator.SetTrigger(DieHash);
        }
        else
        {
            Debug.LogWarning("[PlayerDeath] Không tìm thấy Animator!");
        }

        // 4. Tắt hẳn sau delay
        StartCoroutine(DisableAfterDelay(disableDelay));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        Debug.Log("[PlayerDeath] Player đã bị tắt.");
    }
}
