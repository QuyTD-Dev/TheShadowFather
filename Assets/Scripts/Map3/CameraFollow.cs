using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cài Đặt")]
    public Transform target; // Đây là chỗ để nhét Player vào
    public float smoothSpeed = 0.125f; // Độ mượt (số càng nhỏ càng mượt)
    public Vector3 offset = new Vector3(0, 2, -10); // Vị trí lệch (X, Y, Z)

    void LateUpdate()
    {
        if (target != null)
        {
            // Tính toán vị trí mong muốn
            Vector3 desiredPosition = target.position + offset;
            
            // Dùng hàm Lerp để di chuyển camera từ từ đến vị trí đó
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Gán vị trí mới cho Camera
            transform.position = smoothedPosition;
        }
    }  
}