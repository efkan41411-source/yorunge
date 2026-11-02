using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaLayout : MonoBehaviour
{
    private RectTransform rect;
    private Rect previous;
    private Vector2 previousSize;
    private void Awake() { rect = (RectTransform)transform; Apply(); }
    private void Update() { if (previous != Screen.safeArea || previousSize != new Vector2(Screen.width, Screen.height)) Apply(); }
    private void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return;
        previous = Screen.safeArea; previousSize = new Vector2(Screen.width, Screen.height);
        rect.anchorMin = new Vector2(previous.xMin / Screen.width, previous.yMin / Screen.height);
        rect.anchorMax = new Vector2(previous.xMax / Screen.width, previous.yMax / Screen.height);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
