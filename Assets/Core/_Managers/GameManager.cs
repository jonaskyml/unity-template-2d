using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
