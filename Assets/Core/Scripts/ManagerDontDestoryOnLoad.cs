using UnityEngine;

public class ManagerDontDestroyOnLoad : MonoBehaviour
{
    private static ManagerDontDestroyOnLoad instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // eliminates duplicates
        }
    }
}