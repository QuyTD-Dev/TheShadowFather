using UnityEngine;
using System.Collections;

public class FireTrap : MonoBehaviour
{
    public GameObject bombPrefab;
    public Transform dropPoint;
    public float dropInterval = 2f;

    private Coroutine dropRoutine;
    private Animator anim;

    // BỎ TRỐNG hàm Start, KHÔNG ĐƯỢC gọi thả bom ở đây
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Hàm này chỉ được gọi bởi Boss khi đã sang Phase 3
    public void ActivateTrap()
    {
        if (dropRoutine == null)
        {
            dropRoutine = StartCoroutine(DropBombRoutine());
        }
    }

    public void DeactivateTrap()
    {
        if (dropRoutine != null)
        {
            StopCoroutine(dropRoutine);
            dropRoutine = null;
        }
    }

    IEnumerator DropBombRoutine()
    {
        // Random một chút để các bẫy không rớt cùng 1 nhịp
        yield return new WaitForSeconds(Random.Range(0f, 1f));

        while (true)
        {
            if (anim != null) anim.SetTrigger("Drop");

            if (bombPrefab != null && dropPoint != null)
            {
                Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(dropInterval);
        }
    }
}