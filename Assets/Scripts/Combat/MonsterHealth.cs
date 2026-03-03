using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [Header("Chỉ số của Quái")]
    [SerializeField] private int maxHealth = 100; // Máu tối đa
    private int currentHealth; // Máu hiện tại

    void Start()
    {
        // Khi game bắt đầu, gán máu hiện tại = máu tối đa
        currentHealth = maxHealth;
    }

    // Hàm này sẽ được gọi từ bên ngoài (vũ khí người chơi)
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount; // Trừ máu
        Debug.Log(transform.name + " bị đánh! Máu còn: " + currentHealth);

        // Kiểm tra nếu máu <= 0 thì chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(transform.name + " đã bị tiêu diệt!");
        // Thêm hiệu ứng nổ hoặc âm thanh chết tại đây (nếu có)

        // Xóa object quái khỏi game
        Destroy(gameObject);
    }
}
