using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Liên kết")]
    public PlayerInventory playerInv;
    public GameObject shopPanel;
    public GameObject btnOpenShop;

    [Header("Chữ hiển thị UI (Chỉ hiện số)")]
    public Text goldText;
    public Text hpText;
    public Text mpText;
    public Text antidoteText;
    public Text rageText;

    [Header("Giá Tiền")]
    public int hpPrice = 50;
    public int mpPrice = 50;
    public int antidotePrice = 30;
    public int ragePrice = 100;

    void Start()
    {
        UpdateUI();
        if (shopPanel != null) shopPanel.SetActive(false);
        if (btnOpenShop != null) btnOpenShop.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!shopPanel.activeSelf) OpenShop();
            else CloseShop();
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        if (btnOpenShop != null) btnOpenShop.SetActive(false);
        UpdateUI();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        if (btnOpenShop != null) btnOpenShop.SetActive(true);
    }

    // Các hàm mua đồ
    public void BuyHP() { if (playerInv.gold >= hpPrice) { playerInv.gold -= hpPrice; playerInv.hpPotion++; UpdateUI(); } }
    public void BuyMP() { if (playerInv.gold >= mpPrice) { playerInv.gold -= mpPrice; playerInv.mpPotion++; UpdateUI(); } }
    public void BuyAntidote() { if (playerInv.gold >= antidotePrice) { playerInv.gold -= antidotePrice; playerInv.antidote++; UpdateUI(); } }
    public void BuyRage() { if (playerInv.gold >= ragePrice) { playerInv.gold -= ragePrice; playerInv.ragePotion++; UpdateUI(); } }

    // Hàm cập nhật giao diện - Đã chỉnh lại chỉ hiện số
    // Hàm cập nhật giao diện - Chỉ hiện con số thuần túy
    void UpdateUI()
    {
        // Hiển thị Vàng: Chỉ hiện con số (ví dụ: 500)
        if (goldText != null) goldText.text = playerInv.gold.ToString();

        // Hiển thị Số lượng vật phẩm: Chỉ hiện con số (ví dụ: 0)
        if (hpText != null) hpText.text = playerInv.hpPotion.ToString();
        if (mpText != null) mpText.text = playerInv.mpPotion.ToString();
        if (antidoteText != null) antidoteText.text = playerInv.antidote.ToString();
        if (rageText != null) rageText.text = playerInv.ragePotion.ToString();
    }
}