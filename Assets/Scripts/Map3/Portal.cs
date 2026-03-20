using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Điểm đến")]
    public Transform destinationPoint;

    private bool isTeleporting = false;
    public GameObject cameraToTurnOff;
    public GameObject cameraToTurnOn;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // LOG 1: Bắt được bất cứ cái gì chạm vào cổng
        Debug.Log($"[Portal - {gameObject.name}] Có vật thể chạm vào: {collision.gameObject.name} | Tag hiện tại: {collision.tag}");

        // Kiểm tra 1: Xem có đúng Tag Player không
        if (!collision.CompareTag("Player"))
        {
            Debug.LogWarning($"[Portal - {gameObject.name}] TỪ CHỐI: Vật thể không có tag 'Player'.");
            return; // Dừng luôn, không chạy tiếp
        }

        // Kiểm tra 2: Xem cổng có đang bị khóa không
        if (isTeleporting)
        {
            Debug.LogWarning($"[Portal - {gameObject.name}] TỪ CHỐI: Cổng đang trong thời gian khóa (isTeleporting = true).");
            return;
        }

        // Kiểm tra 3: Xem đã kéo thả cổng đích vào Inspector chưa
        if (destinationPoint == null)
        {
            Debug.LogError($"[Portal - {gameObject.name}] LỖI: Chưa gán Destination Point trong Inspector!");
            return;
        }

        // Vượt qua mọi bài test -> Cho phép dịch chuyển
        Debug.Log($"[Portal - {gameObject.name}] ĐỦ ĐIỀU KIỆN! Bắt đầu dịch chuyển player...");
        StartCoroutine(TeleportProcess(collision.transform));
    }

    IEnumerator TeleportProcess(Transform playerTransform)
    {
        isTeleporting = true;
        Debug.Log($"[Portal - {gameObject.name}] Đang dịch chuyển {playerTransform.name} sang: {destinationPoint.name}");

        // Khóa cổng đích
        Portal destinationPortal = destinationPoint.GetComponentInParent<Portal>();
        if (destinationPortal != null)
        {
            Debug.Log($"[Portal - {gameObject.name}] Đã tìm thấy cổng đích ({destinationPortal.name}), tiến hành khóa nó lại.");
            destinationPortal.LockPortal();
        }
        else
        {
            Debug.Log($"[Portal - {gameObject.name}] Cổng đích không có script Portal, bỏ qua bước khóa cổng đích.");
        }

        // Dịch chuyển
        playerTransform.position = destinationPoint.position;
        Debug.Log($"[Portal - {gameObject.name}] Đã thay đổi vị trí player thành công!");

        // ==========================================
        // THÊM CODE ĐỔI CAMERA VÀO ĐÂY
        // ==========================================
        if (cameraToTurnOff != null)
        {
            cameraToTurnOff.SetActive(false); // Tắt cam map cũ
            Debug.Log($"[Portal] Đã tắt camera: {cameraToTurnOff.name}");
        }

        if (cameraToTurnOn != null)
        {
            cameraToTurnOn.SetActive(true);   // Bật cam map mới
            Debug.Log($"[Portal] Đã bật camera: {cameraToTurnOn.name}");
        }

        yield return new WaitForSeconds(0.5f);

        isTeleporting = false;
        Debug.Log($"[Portal - {gameObject.name}] Đã mở khóa cổng hiện tại.");
    }

    public void LockPortal()
    {
        Debug.Log($"[Portal - {gameObject.name}] Đang bị khóa tạm thời bởi một cổng khác.");
        StartCoroutine(LockRoutine());
    }

    private IEnumerator LockRoutine()
    {
        isTeleporting = true;
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
        Debug.Log($"[Portal - {gameObject.name}] Đã mở khóa trở lại.");
    }
}