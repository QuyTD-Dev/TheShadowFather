using UnityEngine;

namespace TheShadowFather.Player
{
    /// <summary>
    /// Lốc xoáy băng giá của chiêu Until (Frost form).
    /// Gắn vào Prefab lốc xoáy. Khi được spawn sẽ bay thẳng
    /// theo hướng nhân vật và tự phát animation lốc xoáy.
    /// Gây sát thương khi chạm quái.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class FrostTornadoProjectile : MonoBehaviour
    {
        [Header("Cấu hình")]
        [Tooltip("Tốc độ bay (đơn vị/giây)")]
        public float speed = 8f;

        [Tooltip("Sát thương khi trúng quái")]
        public int damage = 45;

        [Tooltip("Thời gian tồn tại trước khi tự huỷ (giây)")]
        public float lifetime = 4f;

        [Header("Kích thước lốc xoáy")]
        [Tooltip("Scale của lốc xoáy")]
        public float tornadoScale = 2.5f;

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

            // Lật sprite + scale
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * tornadoScale * direction;
            scale.y = Mathf.Abs(scale.y) * tornadoScale;
            transform.localScale = scale;

            // Tự huỷ sau lifetime giây
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Enemy")) return;

            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[FrostTornado] Lốc băng trúng {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }

            BossHealth bossHP = other.GetComponent<BossHealth>();
            if (bossHP != null)
            {
                bossHP.TakeDamage(damage);
                Debug.Log($"[FrostTornado] Lốc băng trúng Boss {other.name} → -{damage} HP");
                Destroy(gameObject);
                return;
            }
        }
    }
}
