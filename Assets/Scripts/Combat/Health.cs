using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public UnityEvent<int, int> OnHealthChanged; // Nối với thanh Slider UI
    public UnityEvent OnDied; // Nối với hiệu ứng chết hoặc Game Over

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
        Debug.Log(gameObject.name + " đã hết máu!");
        OnDied?.Invoke();
        // Nếu là Boss thì có thể Destroy, nếu là Player thì hiện bảng Game Over
    }
}