using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    // input
    public InputActionReference togglePanel;

    // panels
    public GameObject panelSettings;
    public GameObject panelPauseMenu;

    private void OnEnable()
    {
        togglePanel.action.Enable();

        togglePanel.action.started += OnClosePanelSettings;
        togglePanel.action.started += OnTogglePanelPauseMenu;
    }

    private void OnDisable()
    {
        togglePanel.action.started -= OnClosePanelSettings;
        togglePanel.action.started -= OnTogglePanelPauseMenu;

        togglePanel.action.Disable();
    }

    public void OpenPanelSettings()
    {
        panelSettings.SetActive(true);
    }

    public void ClosePanelSettings()
    {
        panelSettings.SetActive(false);
    }

    private void OnClosePanelSettings(InputAction.CallbackContext context)
    {
        if (panelSettings.activeSelf)
        {
            ClosePanelSettings();
        }
    }

    private void OnTogglePanelPauseMenu(InputAction.CallbackContext context)
    {
        bool isActive = panelPauseMenu.activeSelf;
        panelPauseMenu.SetActive(!isActive);

        Time.timeScale = isActive ? 1f : 0f;
    }
}
