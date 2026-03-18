using UnityEngine;

public class BossMeleeDamage : MonoBehaviour
{
    [Header("Cài đặt sát thương cận chiến")]
    [Tooltip("Lượng máu sẽ trừ khi chém trúng")]
    public int damageAmount = 30;

    [Tooltip("Tag của đối tượng chịu sát thương")]
    public string targetTag = "Player";

    // Hàm này tự động gọi khi có một vật thể (có Collider) chạm vào MeleeHitbox (Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem người bị chạm có đúng là Player không
        if (collision.CompareTag(targetTag))
        {
            // TÌM SCRIPT MÁU CỦA PLAYER
            // (Đổi chữ 'Health' thành tên script quản lý máu của Elias nếu bạn dùng tên khác, VD: PlayerController)
            Health playerHealth = collision.GetComponent<Health>();

            // Nếu tìm thấy script máu, tiến hành trừ máu
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("<color=orange>Boss 3 chém trúng " + collision.name + " - Trừ " + damageAmount + " HP!</color>");
            }
            else
            {
                Debug.LogWarning("Boss chém trúng Player nhưng không tìm thấy script Health để trừ máu!");
            }
        }
    }
}