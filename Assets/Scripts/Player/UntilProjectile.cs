using UnityEngine;

namespace TheShadowFather.Player
{
    /// <summary>
    /// Viên đạn/lưỡi kiếm năng lượng của chiêu Until.
    /// Gắn vào Prefab projectile. Khi được spawn sẽ bay thẳng
    /// theo hướng nhân vật đứng và gây sát thương khi chạm quái.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class UntilProjectile : MonoBehaviour
    {
        [Header("Cấu hình")]
        [Tooltip("Tốc độ bay (đơn vị/giây)")]
        public float speed = 12f;

        [Tooltip("Sát thương khi trúng quái")]
        public int damage = 30;

        [Tooltip("Thời gian tồn tại trước khi tự huỷ (giây)")]
        public float lifetime = 3f;

        private Rigidbody2D rb;

        /// <summary>
        /// Gọi bởi PlayerController ngay sau khi Instantiate.
        /// direction = 1 (bay phải), -1 (bay trái).
        /// </summary>
        public void Launch(float direction)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Không rơi xuống

            // Bay theo đường thẳng ngang
            rb.linearVelocity = Vector2.right * speed * direction;

            // Lật sprite nếu bay sang trái
            if (direction < 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            // Tự huỷ sau lifetime giây
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Chỉ gây sát thương cho Enemy
            if (!other.CompareTag("Enemy")) return;

            // Thử Health.cs (quái thường)
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[UntilProjectile] Trúng {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }

            // Thử BossHealth.cs (Boss Bear dùng hệ thống riêng)
            BossHealth bossHP = other.GetComponent<BossHealth>();
            if (bossHP != null)
            {
                bossHP.TakeDamage(damage);
                Debug.Log($"[UntilProjectile] Trúng Boss {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }
        }
    }
}
