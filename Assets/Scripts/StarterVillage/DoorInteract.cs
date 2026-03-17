using UnityEngine;
using UnityEngine.SceneManagement; // BẮT BUỘC phải có dòng này để chuyển cảnh

public class DoorInteract : MonoBehaviour
{
    private bool isPlayerNear = false;

    void Update()
    {
        // Khi Player đứng gần và bấm E
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            // Chuyển sang Scene bên trong nhà
            SceneManager.LoadScene("AriaHouse");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNear = false;
    }
}