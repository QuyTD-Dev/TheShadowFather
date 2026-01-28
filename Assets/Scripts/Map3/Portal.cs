using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Điểm đến")]
    public Transform destinationPoint;

    // Biến static để khóa cổng toàn cục (tránh việc vừa sang map 2 lại bị cổng map 2 hút ngược về)
    private static bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Kiểm tra đúng Tag Player
        // 2. Kiểm tra xem có đang trong quá trình dịch chuyển không
        if (collision.CompareTag("Player") && !isTeleporting)
        {
            StartCoroutine(TeleportProcess(collision.transform));
        }
    }

    IEnumerator TeleportProcess(Transform playerTransform)
    {
        isTeleporting = true; // Khóa cổng
        Debug.Log("Bắt đầu dịch chuyển sang: " + destinationPoint.name);

        // Dịch chuyển ngay lập tức
        playerTransform.position = destinationPoint.position;

        // Chờ 0.5 giây để ổn định (tránh lỗi va chạm kép khi kéo chuột)
        yield return new WaitForSeconds(0.5f);

        isTeleporting = false; // Mở khóa
    }
}