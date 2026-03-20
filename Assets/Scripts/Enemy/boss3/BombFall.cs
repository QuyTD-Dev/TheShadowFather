using UnityEngine;

public class BombFall : MonoBehaviour
{
    public int damage = 15;
    public float fallSpeed = 5f;
    public GameObject explosionEffect; // (Tùy chọn) Hiệu ứng nổ khi chạm đất

    void Update()
    {
        // Rơi tự do xuống dưới (nếu không dùng Rigidbody2D Gravity)
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Gọi hàm nhận sát thương của Player (Giả sử tên hàm là TakeDamage)
            // collision.GetComponent<PlayerHealth>().TakeDamage(damage);
            DestroyBomb();
        }
        else if (collision.CompareTag("Ground")) // Chạm mặt đất
        {
            DestroyBomb();
        }
    }

    void DestroyBomb()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}