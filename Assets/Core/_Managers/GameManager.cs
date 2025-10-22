using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // scenes
    public string startGameScene;
    public string startTutorialScene;

    public void StartGame()
    {
        SceneManager.LoadScene(startGameScene);
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(startTutorialScene);
    }
}
