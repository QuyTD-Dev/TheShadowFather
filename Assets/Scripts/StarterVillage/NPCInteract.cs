using UnityEngine;
using TMPro;
using System.Collections;

// Class này giúp bạn tạo ra các dòng thoại có tên người nói trong Inspector
[System.Serializable]
public struct DialogueLine
{
    public string speakerName; // Tên người nói (VD: Player hoặc Kael)
    [TextArea(3, 5)]
    public string sentence;    // Nội dung câu thoại
}

public class NPCInteract : MonoBehaviour
{
    [Header("Cấu hình đối thoại")]
    public DialogueLine[] conversation; // Thay thế mảng string cũ

    [Header("Cấu hình thời gian")]
    public float delayBetweenLines = 3f;

    [Header("Kéo thả UI vào đây")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private bool isPlayerNearby = false;
    private bool isTalking = false;
    private Coroutine autoPlayRoutine;

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !isTalking)
        {
            StartAutoDialogue();
        }
    }

    void StartAutoDialogue()
    {
        if (dialoguePanel == null || dialogueText == null || conversation.Length == 0) return;

        isTalking = true;
        dialoguePanel.SetActive(true);
        autoPlayRoutine = StartCoroutine(PlayDialogueRoutine());
    }

    IEnumerator PlayDialogueRoutine()
    {
        for (int i = 0; i < conversation.Length; i++)
        {
            // Hiển thị tên người nói đậm hơn và nội dung câu thoại
            dialogueText.text = "<b>" + conversation[i].speakerName + ":</b> " + conversation[i].sentence;

            yield return new WaitForSeconds(delayBetweenLines);
        }
        EndDialogue();
    }

    void EndDialogue()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (autoPlayRoutine != null) StopCoroutine(autoPlayRoutine);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            EndDialogue();
        }
    }
}