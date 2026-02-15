using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject mainMenuBackground;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject panelSettings;
    [SerializeField] private GameObject panelCredits;
    [SerializeField] private GameObject skipButton;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float sfxFadeDuration = 0.2f;
    
    [Header("Input Actoins")]
    [SerializeField] private InputActionReference pauseActionPlayer;
    [SerializeField] private InputActionReference pauseActionUI;

    // [Header("Scenes")]
    // public string mainMenuScene;
    // public string startGameScene;
    // public string startTutorialScene;
    
    private static GameManager instance;
    private bool IsPaused => pauseMenu?.activeSelf ?? false;
    private bool IsMainMenuScene => SceneManager.GetActiveScene().buildIndex == 0;

    private Coroutine sfxFadeCoroutine;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destory(gameObject);
            return;
        }
    
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        EnablePauseActions();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        DisablePauseActions();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void EnablePauseActions()
    {
        if (pauseActionPlayer != null)
        {
            pauseActionPlayer.action.started += TogglePause;
            pauseActionPlayer.action.Enable();
        }

        if (pauseActionUI != null)
        {
            pauseActionUI.action.started += TogglePause;
            pauseActionUI.action.Enable();
        }
    }

    private void DisablePauseActions()
    {
        if (pauseActionPlayer != null)
        {
            pauseActionPlayer.action.started -= TogglePause;
            pauseActionPlayer.action.Disable();
        }

        if (pauseActionUI != null)
        {
            pauseActionUI.action.started -= TogglePause;
            pauseActionUI.action.Disable();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isMenu = IsMainMenuScene;
        bool isLevel1 = scene.name == "Level1";
        bool isCredits = scene.name.ToLower() == "credits";

        // Reset UI state when returning to main menu
        if (isMenu)
        {
            SetPanelActive(pauseMenu, false);
            SetPanelActive(panelSettings, false);
            SetPanelActive(panelCredits, false);
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(mainMenuBackground, true);
            
            // Refresh ASyncLoader references when returning to main menu
            ASyncLoader loader = FindFirstObjectByType<ASyncLoader>();
            if (loader != null)
            {
                loader.RefreshMainMenuReferences();
            }
        }
        
        // Handle Credits scene
        if (isCredits)
        {
            SetPanelActive(pauseMenu, false);
            SetPanelActive(panelSettings, false);
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(mainMenuBackground, false);
            SetPanelActive(panelCredits, true);
            Debug.Log("Credits scene loaded - showing credits panel");
        }
        
        if (skipButton != null)
            skipButton.SetActive(isCredits);

        ASyncLoader asyncLoader = FindFirstObjectByType<ASyncLoader>();
        bool isLoading = asyncLoader != null && asyncLoader.IsLoading();

        UpdatePauseInput(!isMenu && !isCredits && !isLoading);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
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
