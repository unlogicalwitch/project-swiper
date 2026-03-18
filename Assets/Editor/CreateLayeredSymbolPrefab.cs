using UnityEditor;
using UnityEngine;

public class CreateLayeredSymbolPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/LayeredSymbolObject.prefab";

        GameObject temp = new GameObject("LayeredSymbolObject");
        SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        temp.AddComponent<LayeredFallingSymbol>();

        PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
        Object.DestroyImmediate(temp);

        AssetDatabase.SaveAssets();
        Debug.Log("LayeredSymbolObject prefab created at " + prefabPath);
    }
}
