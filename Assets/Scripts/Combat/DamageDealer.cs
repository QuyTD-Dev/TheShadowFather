using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private int damage = 20; // Sát thương gây ra

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật va chạm có phải là Quái không (thông qua Tag)
        if (other.CompareTag("Enemy"))
        {
            // Tìm component MonsterHealth trên người con quái
            MonsterHealth enemyHealth = other.GetComponent<MonsterHealth>();

            // Nếu tìm thấy script máu, thì gọi hàm trừ máu
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }
}
