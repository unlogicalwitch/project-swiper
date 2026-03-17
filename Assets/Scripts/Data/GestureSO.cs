using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gesture/Gesture Template")]
public class GestureSO : ScriptableObject
{
    public string gestureID;
    public Sprite gestureSprite;
    [Tooltip("Points should be drawn in correct direction & orientation")]
    public List<Vector2> points;
}