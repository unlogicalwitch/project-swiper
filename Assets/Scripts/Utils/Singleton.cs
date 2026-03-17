using UnityEngine;

/// <summary>
/// Generic scene-scoped singleton base class.
/// 
/// Scene-scoped by design: the instance is NOT preserved across scene loads.
/// This is intentional — RestartGame() reloads the scene, which re-creates
/// all managers cleanly. If cross-scene persistence is needed in the future,
/// uncomment DontDestroyOnLoad in Awake and update RestartGame accordingly.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<T>();

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = this as T;
        else
            Destroy(gameObject);
    }
}
