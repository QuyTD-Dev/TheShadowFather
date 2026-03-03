using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 15;
    public float lifetime = 4f;

    private Rigidbody2D rb;

    // Đổi Start thành SetDirection để nhận lệnh từ Boss
    public void SetDirection(float facingDirection)
    {
        rb = GetComponent<Rigidbody2D>();

        // Đẩy viên đạn bay theo hướng được truyền vào (1 là phải, -1 là trái)
        rb.linearVelocity = transform.right * speed * facingDirection;

        // Lật hình ảnh viên đạn lại nếu nó đang bay sang trái
        Vector3 scale = transform.localScale;
        scale.x = facingDirection;
        transform.localScale = scale;

        // Tự hủy đạn
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[2D] Trúng đạn! Trừ {damage} máu.");
            // other.GetComponent<PlayerHealth>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}