using System.Collections;
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("---- Cấu hình Máu ----")]
    public float phase1Health = 100f;
    public float phase2Health = 100f;

    [Header("---- Trạng thái (Kéo về 0 để test) ----")]
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isPhase2 = false;
    [SerializeField] private bool isInvulnerable = false;

    private BossController bossCtrl;
    private Animator anim;

    void Start()
    {
        bossCtrl = GetComponent<BossController>();
        anim = GetComponent<Animator>();
        currentHealth = phase1Health;
    }

    void Update()
    {
        // --- ĐÃ XÓA ĐOẠN INPUT.GETKEYDOWN(KEYCODE.K) ĐỂ TRÁNH LỖI ---

        // Bạn chỉ cần dùng chuột kéo thanh Current Health trong Inspector về 0 là được
        if (!isInvulnerable)
        {
            // Tự động phát hiện khi bạn kéo máu về 0
            if (currentHealth <= 0)
            {
                if (!isPhase2)
                {
                    StartCoroutine(TransactionToPhase2());
                }
                else
                {
                    Die();
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"[BOSS HP] Bị đánh! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            if (!isPhase2) StartCoroutine(TransactionToPhase2());
            else Die();
        }
    }

    IEnumerator TransactionToPhase2()
    {
        if (isPhase2) yield break;

        Debug.Log("<color=yellow>[BOSS HP] Hết máu P1 -> Bắt đầu Biến hình!</color>");

        isInvulnerable = true;
        currentHealth = 0;

        if (bossCtrl != null) bossCtrl.StartPhase2();

        yield return new WaitForSeconds(3.0f);

        currentHealth = phase2Health;
        isPhase2 = true;
        isInvulnerable = false;
        Debug.Log($"<color=red>[BOSS HP] Đã sang Phase 2! Máu hồi lại: {currentHealth}</color>");
    }

    void Die()
    {
        if (bossCtrl != null) bossCtrl.Die();
        Debug.Log("<color=red>[BOSS HP] CHẾT HẲN!</color>");
        this.enabled = false;
    }
}
