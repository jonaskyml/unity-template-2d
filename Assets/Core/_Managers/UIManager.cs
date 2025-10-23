using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Input")]
    public InputActionReference togglePanel;
    public InputActionReference closePanelSettings;

    [Header("Panels")]
    public GameObject panelSettings;
    public GameObject panelPause;

    private void Awake()
    {
        // assign once in awake
        TryAssignPanels();

        // re-assign whenever a new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryAssignPanels();
    }

    private void TryAssignPanels()
    {
        if (panelPause == null)
        {
            GameObject found = FindObjectByName("PanelPauseMenu");
            if (found != null)
            {
                panelPause = found;
                Debug.Log("[autoassign] found and assigned PanelPauseMenu");
            }
        }
    }

    private GameObject FindObjectByName(string name)
    {
        // searches across all loaded scenes, including inactive objects
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == name && !string.IsNullOrEmpty(go.scene.name))
                return go;
        }
        return null;
    }

    private void OnEnable()
    {
        if (togglePanel != null)
        {
            togglePanel.action.Enable();
            togglePanel.action.started += OnTogglePause;
        }

        if (closePanelSettings != null )
        {
            closePanelSettings.action.Enable();
            closePanelSettings.action.started += OnCloseSettings;
        }
    }

    private void OnDisable()
    {
        if (togglePanel != null && togglePanel.action != null)
            togglePanel.action.started -= OnTogglePause;

        if (closePanelSettings != null && closePanelSettings.action != null)
            closePanelSettings.action.started -= OnCloseSettings;

        if (togglePanel != null && togglePanel.action != null)
            togglePanel.action.Disable();

        if (closePanelSettings != null && closePanelSettings.action != null)
            closePanelSettings.action.Disable();
    }

    // === pause ===
    private void OnTogglePause(InputAction.CallbackContext ctx)
    {
        if (panelPause == null) return;

        bool isActive = panelPause.activeSelf;
        panelPause.SetActive(!isActive);
        GameManager.Instance.TogglePause();
    }


    // === settings ===
    public void OpenSettings() => panelSettings?.SetActive(true);
    public void CloseSettings() => panelSettings?.SetActive(false);

    private void OnCloseSettings(InputAction.CallbackContext ctx)
    {
        if (panelSettings == null) return;
        if (!panelSettings.activeSelf) return;

        CloseSettings();
    }
}
