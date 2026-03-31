using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gesture/Gesture Template")]
public class GestureSO : ScriptableObject
{
    public string gestureID;
    public Sprite gestureSprite;

    [Tooltip("Frames cycled by BoilingAnimator to give the symbol a hand-drawn boiling look. " +
             "Leave empty to show a still sprite.")]
    public Sprite[] boilingFrames;

    [Tooltip("Points should be drawn in correct direction & orientation")]
    public List<Vector2> points;
}
