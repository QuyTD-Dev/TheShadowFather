using UnityEngine;

/// <summary>
/// Gắn vào child object "AttackHitbox" của quái.
/// Khi được bật (SetActive) và chạm Player → gây sát thương.
/// EnemyAI sẽ bật/tắt object này qua Animation Event hoặc code.
/// </summary>
public class EnemyMeleeAttack : MonoBehaviour
{
    [Tooltip("Sát thương mỗi lần đánh")]
    public int damage = 15;

    [Tooltip("Cooldown (giây) để tránh đánh trúng nhiều lần trong 1 animation")]
    public float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            lastHitTime = Time.time;
            Debug.Log($"[EnemyMelee] {transform.parent.name} đánh Player → -{damage} HP");
        }
    }

    /// <summary>Gọi từ Animation Event để bật hitbox.</summary>
    public void EnableHitbox() => gameObject.SetActive(true);

    /// <summary>Gọi từ Animation Event để tắt hitbox.</summary>
    public void DisableHitbox() => gameObject.SetActive(false);
}
