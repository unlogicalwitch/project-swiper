using UnityEngine;

[CreateAssetMenu(menuName = "Gesture/Gesture Library")]
public class GestureLibrary : ScriptableObject
{
    [SerializeField] private GestureSO[] gestures;

    /// <summary>Returns all registered gesture templates.</summary>
    public GestureSO[] GetAllGestures() => gestures;

    /// <summary>Returns a random gesture from the library.</summary>
    public GestureSO GetRandomGesture()
    {
        if (gestures == null || gestures.Length == 0)
        {
            Debug.LogError("GestureLibrary: no gestures assigned!");
            return null;
        }

        return gestures[Random.Range(0, gestures.Length)];
    }
}
