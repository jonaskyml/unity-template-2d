using UnityEngine;

public class ManagerDontDestroyOnLoad : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
