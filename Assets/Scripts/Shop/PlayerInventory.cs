using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Tài sản hiện có")]
    public int gold = 500; // Cho sẵn 500 vàng để bạn dễ test việc mua sắm

    [Header("Số lượng vật phẩm")]
    public int hpPotion = 0;     // Bình Máu
    public int mpPotion = 0;     // Bình Năng lượng
    public int antidote = 0;     // Thuốc Giải độc
    public int ragePotion = 0;   // Thuốc Cuồng nộ
}