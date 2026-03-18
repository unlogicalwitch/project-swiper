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

    /// <summary>
    /// Returns an array of N unique random gestures.
    /// If count exceeds the library size, returns all gestures shuffled.
    /// </summary>
    public GestureSO[] GetUniqueRandomGestures(int count)
    {
        if (gestures == null || gestures.Length == 0)
        {
            Debug.LogError("GestureLibrary: no gestures assigned!");
            return null;
        }

        // Clamp to available gestures
        count = Mathf.Min(count, gestures.Length);

        // Fisher-Yates shuffle on a copy
        GestureSO[] shuffled = (GestureSO[])gestures.Clone();
        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        GestureSO[] result = new GestureSO[count];
        System.Array.Copy(shuffled, result, count);
        return result;
    }
}
