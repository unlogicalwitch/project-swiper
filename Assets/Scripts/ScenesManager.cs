using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : Singleton<ScenesManager>
{
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    public void ReloadScene()
    {
        StartCoroutine(LoadSceneCoroutine(SceneManager.GetActiveScene().name));
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 1. Play transition out (fade, etc.)
        // 2. Optionally show a loading screen
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // 3. Scene is ready — activate it
        op.allowSceneActivation = true;
        yield return op;

        // 4. Play transition in
    }
} 