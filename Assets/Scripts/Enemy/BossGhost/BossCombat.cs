using UnityEngine;
using System.Collections;

public class BossCombat : MonoBehaviour
{
    [Header("Melee Setup (Attack/Skill)")]
    public Transform meleePoint;
    public float meleeRadius = 1.5f;
    public int attackDamage = 20;
    public int skillDamage = 40;
    public LayerMask playerLayer;

    [Header("Ranged Setup (Summon)")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float spreadAngle = 15f;

    public void PerformAttack(float delay)
    {
        StartCoroutine(DelayDamage(delay, attackDamage));
    }

    public void PerformSkill(float delay)
    {
        StartCoroutine(DelayDamage(delay, skillDamage));
    }

    private IEnumerator DelayDamage(float delay, int damage)
    {
        yield return new WaitForSeconds(delay);

        if (meleePoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(meleePoint.position, meleeRadius, playerLayer);
            foreach (Collider2D hit in hits)
            {
                Debug.Log($"[2D] Trúng đòn cận chiến! Trừ {damage} máu.");
            }
        }
    }

    // 1. CẬP NHẬT: Bắn 1 viên
    public void SpawnBullet()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // Xác định Boss đang nhìn hướng nào (1 = phải, -1 = trái)
            float facing = Mathf.Sign(transform.lossyScale.x);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            // Truyền hướng cho đạn
            bullet.GetComponent<BossBullet>().SetDirection(facing);

            Debug.Log("Boss đã triệu hồi đạn 2D!");
        }
    }

    // 2. CẬP NHẬT: Bắn 3 viên tỏa ra
    public void SpawnSpreadBullets()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Quaternion baseRot = firePoint.rotation;
            float facing = Mathf.Sign(transform.lossyScale.x);

            // Viên thứ 1: Bay thẳng
            GameObject b1 = Instantiate(bulletPrefab, firePoint.position, baseRot);
            b1.GetComponent<BossBullet>().SetDirection(facing);

            // Viên thứ 2: Chếch lên trên (nhân với facing để khi lật mặt, góc tỏa cũng lật theo)
            Quaternion spreadUp = baseRot * Quaternion.Euler(0, 0, spreadAngle * facing);
            GameObject b2 = Instantiate(bulletPrefab, firePoint.position, spreadUp);
            b2.GetComponent<BossBullet>().SetDirection(facing);

            // Viên thứ 3: Chếch xuống dưới
            Quaternion spreadDown = baseRot * Quaternion.Euler(0, 0, -spreadAngle * facing);
            GameObject b3 = Instantiate(bulletPrefab, firePoint.position, spreadDown);
            b3.GetComponent<BossBullet>().SetDirection(facing);

            Debug.Log("Skill kích hoạt: Bắn 3 viên đạn tỏa ra!");
        }
    }

    [Header("Ultimate Setup (Bullet Rain)")]
    public int rainBulletCount = 10;
    public float rainDuration = 2f;
    public float rainHeight = 8f;
    public float rainWidth = 6f;

    public void TriggerBulletRain()
    {
        StartCoroutine(SpawnBulletRainRoutine());
    }

    // 3. CẬP NHẬT: Mưa đạn
    private IEnumerator SpawnBulletRainRoutine()
    {
        Debug.Log("Chiêu Cuối: Kích hoạt Mưa Đạn!");
        float delayBetweenBullets = rainDuration / rainBulletCount;

        for (int i = 0; i < rainBulletCount; i++)
        {
            if (bulletPrefab != null)
            {
                float randomX = Random.Range(-rainWidth / 2f, rainWidth / 2f);
                Vector3 spawnPos = transform.position + new Vector3(randomX, rainHeight, 0);
                Quaternion fallRotation = Quaternion.Euler(0, 0, -90f);

                GameObject rainBullet = Instantiate(bulletPrefab, spawnPos, fallRotation);
                // Vì mưa rơi từ trên trời cắm xuống đất nên ta luôn truyền 1 (để đạn bay đúng chiều mũi tên của nó)
                rainBullet.GetComponent<BossBullet>().SetDirection(1f);
            }
            yield return new WaitForSeconds(delayBetweenBullets);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (meleePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, meleeRadius);
        }
    }
}