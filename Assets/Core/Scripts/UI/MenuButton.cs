using UnityEngine;

public class MenuButton : MonoBehaviour
{
    private UIManager uiManager;

    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

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
        uiManager?.OpenSettings();
    }
    
    public void RequestCloseSettings()
    {
        uiManager?.CloseSettings();
    }

    public void RequestResume()
    {
        if (uiManager != null)
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