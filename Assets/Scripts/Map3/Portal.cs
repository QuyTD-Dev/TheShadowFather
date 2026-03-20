using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("CỔNG ĐÍCH ĐẾN")]
    [Tooltip("Kéo Object Cổng đích vào đây")]
    public Portal destinationPortal;

    [Header("ĐIỂM XUẤT HIỆN Ở CỔNG NÀY")]
    [Tooltip("Kéo Object SpawnPoint con của cổng NÀY vào đây")]
    public Transform spawnPoint;

    [Header("CAMERA ZONE (Chỉ dành cho Cổng Cuối)")]
    [Tooltip("Kéo BoxCollider2D vùng camera cảnh TIẾP THEO vào đây")]
    public BoxCollider2D nextCameraZone;

    private bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isTeleporting) return;

        if (destinationPortal == null)
        {
            Debug.LogError($"[Portal] Lỗi: {gameObject.name} chưa gán Destination Portal!");
            return;
        }

        StartCoroutine(TeleportProcess(collision.transform));
    }

    IEnumerator TeleportProcess(Transform playerTransform)
    {
        isTeleporting = true;

        // 1. Khóa cổng đích
        destinationPortal.LockPortal();

        // 2. Dịch chuyển Player đến điểm an toàn
        if (destinationPortal.spawnPoint != null)
        {
            playerTransform.position = destinationPortal.spawnPoint.position;
        }
        else
        {
            playerTransform.position = destinationPortal.transform.position;
            Debug.LogWarning($"[Portal] {destinationPortal.name} bị thiếu Spawn Point!");
        }

        // ========================================================
        // 3. XỬ LÝ SỬA LỖI CAMERA LEM MAP (CRITICAL FIX)
        // ========================================================
        if (nextCameraZone != null)
        {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                // A. Cập nhật giới hạn mới cho Camera trước
                camFollow.SetNewBounds(nextCameraZone);

                // B. Tự tính toán tọa độ an toàn (Clamped) cho Camera ở vùng mới NGAY LẬP TỨC
                Camera cam = Camera.main;
                float camHeight = cam.orthographicSize;
                float camWidth = cam.orthographicSize * cam.aspect;

                Bounds bounds = nextCameraZone.bounds;

                // Tính toán 4 bức tường giới hạn dựa trên Zone mới
                float minX = bounds.min.x + camWidth;
                float maxX = bounds.max.x - camWidth;
                float minY = bounds.min.y + camHeight;
                float maxY = bounds.max.y - camHeight;

                // Xử lý an toàn nếu Map quá nhỏ so với màn hình
                if (minX > maxX) minX = maxX = bounds.center.x;
                if (minY > maxY) minY = maxY = bounds.center.y;

                // C. Lấy tọa độ muốn đến (tọa độ Elias + offset của CameraFollow)
                // Cần lấy offset từ CameraFollow để đảm bảo tính đồng bộ
                Vector3 targetDesiredPos = playerTransform.position + camFollow.offset;

                // D. ÉP BIÊN (CLAMP) tọa độ đó vĩnh viễn nằm trong Zone mới
                float clampedX = Mathf.Clamp(targetDesiredPos.x, minX, maxX);
                float clampedY = Mathf.Clamp(targetDesiredPos.y, minY, maxY);

                // E. Cưỡng ép Camera nhảy đến tọa độ ĐÃ ĐƯỢC ÉP BIÊN AN TOÀN
                cam.transform.position = new Vector3(clampedX, clampedY, cam.transform.position.z);
            }
        }

        // Delay 0.5s để Player kịp di chuyển ra khỏi vùng Trigger cổng đích
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }

    public void LockPortal()
    {
        StartCoroutine(LockRoutine());
    }

    private IEnumerator LockRoutine()
    {
        isTeleporting = true;
        yield return new WaitForSeconds(0.5f);
        isTeleporting = false;
    }
}