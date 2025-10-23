using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePanelManager : MonoBehaviour
{
    public string mainMenuScene;

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
