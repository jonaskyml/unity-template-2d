using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // input
    public InputActionReference closePanel;

    // panels
    public GameObject panelSettings;

    // scenes
    public string startGameScene;
    public string startTutorialScene;

    private void OnEnable()
    {
        closePanel.action.Enable();

        closePanel.action.started += OnClosePanel;
    }

    private void OnDisable()
    {
        closePanel.action.Disable();

        closePanel.action.started -= OnClosePanel;
    }

    public void OpenPanelSettings()
    {
        panelSettings.SetActive(true);
    }

    public void ClosePanelSettings()
    {
        panelSettings.SetActive(false);
    }

    private void OnClosePanel(InputAction.CallbackContext context)
    {
        if (panelSettings.activeSelf)
        {
            ClosePanelSettings();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(startGameScene);
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(startTutorialScene);
    }
}
