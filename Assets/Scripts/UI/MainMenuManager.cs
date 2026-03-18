using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string firstSceneName = "StarterVillage";
    public string tutorialSceneName = "Tutorial";

    public void PlayGame()
    {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene(firstSceneName);
    }

    public void OpenTutorial()
    {
        Debug.Log("Opening Tutorial...");
        SceneManager.LoadScene(tutorialSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    public void BackToMainMenu()
    {
        // "MainMenu" phải khớp chính xác với tên Scene bạn đã đặt trong Project
        SceneManager.LoadScene("MainMenu");
    }
}
