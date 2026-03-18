using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AutoDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;
    public float timeBetweenSentences = 2.0f; // Thời gian chờ trước khi sang câu mới

    [System.Serializable]
    public struct DialogueLine
    {
        public string name;
        [TextArea(3, 10)]
        public string sentence;
    }

    public List<DialogueLine> lines;
    private int index = 0;

    void Start()
    {
        dialoguePanel.SetActive(true);
        StartCoroutine(StartCinematicDialogue());
    }

    IEnumerator StartCinematicDialogue()
    {
        while (index < lines.Count)
        {
            yield return StartCoroutine(TypeSentence(lines[index].sentence));

            // Đợi một khoảng thời gian sau khi chữ chạy xong rồi mới sang câu mới
            yield return new WaitForSeconds(timeBetweenSentences);

            index++;
        }

        // Kết thúc tất cả câu thoại
        dialoguePanel.SetActive(false);
        SceneManager.LoadScene("StarterVillage");
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}