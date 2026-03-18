using UnityEngine;
using System.Collections;

public class FireTrap : MonoBehaviour
{
    public GameObject bombPrefab;
    public Transform dropPoint;
    public float dropInterval = 2f;

    private Coroutine dropRoutine;
    private Animator anim; // Tùy chọn để rung lắc bẫy trước khi rớt

    void Start()
    {
        anim = GetComponent<Animator>();
    }

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
        // Đợi chênh lệch một chút để các bẫy không rớt đạn cùng một lúc
        yield return new WaitForSeconds(Random.Range(0f, 1f));

        while (true)
        {
            if (anim != null) anim.SetTrigger("Drop"); // Gọi animation Ceilling Trap

            Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(dropInterval);
        }
    }
}