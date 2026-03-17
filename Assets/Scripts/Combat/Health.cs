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

    public UnityEvent<int, int> OnHealthChanged; // Nối với thanh Slider UI
    public UnityEvent OnDied;                    // Nối với PlayerDeath.OnPlayerDied hoặc effect chết

    private void Start()
    {
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