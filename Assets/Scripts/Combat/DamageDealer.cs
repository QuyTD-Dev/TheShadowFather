using UnityEngine;

/// <summary>
/// Gắn lên Attack Hitbox của Player.
/// Khi chạm vào object có tag "Enemy" → gọi TakeDamage().
/// Không cần sửa code cũ nào.
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Header("Cấu hình sát thương")]
    public int damage = 20;
    public string targetTag = "Enemy";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(targetTag)) return;

        // Thử Health.cs trước (quái thường + Boss Ghost)
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"[DamageDealer] Đánh trúng {other.name} → -{damage} HP");
            return;
        }

        // Boss Bear dùng BossHealth.cs riêng (giữ 2-phase logic)
        BossHealth bossHP = other.GetComponent<BossHealth>();
        if (bossHP != null)
        {
            bossHP.TakeDamage(damage);
            Debug.Log($"[DamageDealer] Đánh trúng Boss {other.name} → -{damage} HP");
        }
    }
}
