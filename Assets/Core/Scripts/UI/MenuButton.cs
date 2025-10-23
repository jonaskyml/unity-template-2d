using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public void RequestPlayGame()
    {
        GameManager.Instance.StartGame();
    }

    public void RequestTutorial()
    {
        GameManager.Instance.StartTutorial();
    }

    public void RequestMainMenu()
    {
        GameManager.Instance.MainMenu();
    }

    public void RequestOpenSettings()
    {
        UIManager.Instance.OpenSettings();
    }
    
    public void RequestCloseSettings()
    {
        UIManager.Instance.CloseSettings();
    }

    public void RequestResume()
    {
        if (UIManager.Instance != null)
        {
            // this will depend on how is closing the pause menu handled
            GameManager.Instance.TogglePause();
        }
    }

    public void RequestQuit()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}