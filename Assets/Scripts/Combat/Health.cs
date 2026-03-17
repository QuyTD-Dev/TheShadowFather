using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Cấu hình")]
    public int maxHealth = 100;

    [Tooltip("Tick vào nếu là quái/boss → tự Destroy khi chết.\nĐể trống nếu là Player (xử lý qua OnDied event).")]
    public bool destroyOnDeath = false;

    [Tooltip("Delay (giây) trước khi Destroy (để animation chết phát xong)")]
    public float destroyDelay = 0.5f;

    private int currentHealth;
    /// <summary>Máu hiện tại (chỉ đọc).</summary>
    public int CurrentHealth => currentHealth;

    public UnityEvent<int, int> OnHealthChanged; // Nối với thanh Slider UI
    public UnityEvent OnDied;                    // Nối với PlayerDeath.OnPlayerDied hoặc effect chết

    private void Awake()
    {
        // Quái: nối health bar sớm vì EnemyHealthBar là child
        if (GetComponent<TheShadowFather.Player.PlayerController>() == null)
        {
            EnemyHealthBar ehb = GetComponentInChildren<EnemyHealthBar>(true);
            if (ehb != null)
            {
                OnHealthChanged.RemoveAllListeners();
                OnHealthChanged.AddListener(ehb.UpdateBar);
                Debug.Log($"[Health] Tự động nối EnemyHealthBar cho {gameObject.name}!");
            }
        }
    }

    private void Start()
    {
        // Player: nối health bar ở Start (chờ scene objects load xong)
        if (GetComponent<TheShadowFather.Player.PlayerController>() != null)
        {
            // Đảm bảo tag Player
            if (!CompareTag("Player"))
            {
                gameObject.tag = "Player";
                Debug.Log("[Health] Tự động set tag Player!");
            }

            // Thử tìm PlayerHealthBar: trong children trước, rồi mới tìm toàn scene
            PlayerHealthBar phb = GetComponentInChildren<PlayerHealthBar>(true);
            if (phb == null)
                phb = FindFirstObjectByType<PlayerHealthBar>();

            if (phb != null)
            {
                OnHealthChanged.RemoveAllListeners();
                OnHealthChanged.AddListener(phb.UpdateBar);
                Debug.Log("[Health] ✅ Tự động nối PlayerHealthBar thành công!");
            }
            else
            {
                Debug.LogWarning("[Health] ⚠️ Không tìm thấy PlayerHealthBar trong scene!");
            }
        }

        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[Health] {gameObject.name} đã hết máu!");

        // Bắn event (Player dùng cái này để trigger animation Die)
        OnDied?.Invoke();

        // Quái/Boss: tự Destroy sau delay (cho animation chết phát xong)
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}