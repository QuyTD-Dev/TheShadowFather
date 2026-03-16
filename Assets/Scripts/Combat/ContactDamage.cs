using UnityEngine;

/// <summary>
/// Gắn lên prefab đạn của Boss/quái và SpikeTrap.
/// Khi chạm Player → gọi Health.TakeDamage().
/// Không cần sửa code cũ trong BossBullet/BossSkillObject/EnemyBullet/SpikeTrap.
/// </summary>
public class ContactDamage : MonoBehaviour
{
    [Header("Cấu hình")]
    [Tooltip("Sát thương gây cho Player mỗi lần chạm")]
    public int damage = 15;

    [Tooltip("True: hủy object sau khi trúng (đạn). False: giữ lại (SpikeTrap)")]
    public bool destroyOnHit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log($"[ContactDamage] {gameObject.name} trúng Player → -{damage} HP");
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
