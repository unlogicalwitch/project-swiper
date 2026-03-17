using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GestureTrail : MonoBehaviour
{
    LineRenderer line;
    Camera cam;

    public float minDistance = 0.05f;

    Vector3 lastWorldPos;
    bool drawing;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        cam = Camera.main;

        line.positionCount = 0;
        line.useWorldSpace = true;
    }

    public void Begin()
    {
        drawing = true;
        line.positionCount = 0;
        lastWorldPos = Vector3.zero;
    }

    public void AddPoint(Vector2 screenPos)
    {
        if (!drawing) return;

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane + 1f));

        if (line.positionCount > 0 &&
            Vector3.Distance(lastWorldPos, worldPos) < minDistance)
            return;

        line.positionCount++;
        line.SetPosition(line.positionCount - 1, worldPos);
        lastWorldPos = worldPos;
        
        //Debug.Log(worldPos);
    }

    public void End()
    {
        drawing = false;
        Invoke(nameof(Clear), 0.15f);
    }

    void Clear()
    {
        line.positionCount = 0;
    }
}