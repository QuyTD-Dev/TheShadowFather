using UnityEngine;

namespace TheShadowFather.Player
{
    /// <summary>
    /// Rồng lửa của chiêu Until (Fire form).
    /// Gắn vào Prefab rồng lửa. Khi được spawn sẽ bay thẳng
    /// theo hướng nhân vật và tự phát animation rồng bay.
    /// Gây sát thương khi chạm quái.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class FireDragonProjectile : MonoBehaviour
    {
        [Header("Cấu hình")]
        [Tooltip("Tốc độ bay (đơn vị/giây)")]
        public float speed = 10f;

        [Tooltip("Sát thương khi trúng quái")]
        public int damage = 50;

        [Tooltip("Thời gian tồn tại trước khi tự huỷ (giây)")]
        public float lifetime = 4f;

        [Header("Kích thước rồng")]
        [Tooltip("Scale của rồng lửa (to hơn lưỡi kiếm thường)")]
        public float dragonScale = 1.5f;

        private Rigidbody2D rb;

        /// <summary>
        /// Gọi bởi PlayerController ngay sau khi Instantiate.
        /// direction = 1 (bay phải), -1 (bay trái).
        /// </summary>
        public void Launch(float direction)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            // Bay theo đường thẳng ngang
            rb.linearVelocity = Vector2.right * speed * direction;

            // Lật sprite + scale khi bay sang trái
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * dragonScale * direction;
            scale.y = Mathf.Abs(scale.y) * dragonScale;
            transform.localScale = scale;

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
                Debug.Log($"[FireDragon] Rồng lửa trúng {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }

            // Thử BossHealth.cs (Boss Bear)
            BossHealth bossHP = other.GetComponent<BossHealth>();
            if (bossHP != null)
            {
                bossHP.TakeDamage(damage);
                Debug.Log($"[FireDragon] Rồng lửa trúng Boss {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }
        }
    }
}
