using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(GestureSO))]
public class GestureEditor : Editor
{
    GestureSO _gestureTemplate;

    private void OnEnable()
    {
        _gestureTemplate = target as GestureSO;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (_gestureTemplate.gestureSprite == null)
            return;

        Texture2D sprite = AssetPreview.GetAssetPreview(_gestureTemplate.gestureSprite);
        GUILayout.Label("", GUILayout.Height(120), GUILayout.Width(120));
        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), sprite);
    }
}


