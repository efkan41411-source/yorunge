using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float pressedScale = 0.96f;
    [SerializeField] private float response = 20f;
    private bool pressed;
    public void OnPointerDown(PointerEventData eventData) { pressed = true; }
    public void OnPointerUp(PointerEventData eventData) { pressed = false; }
    public void OnPointerExit(PointerEventData eventData) { pressed = false; }
    private void OnDisable() { pressed = false; transform.localScale = Vector3.one; }
    private void Update() { transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (pressed ? pressedScale : 1f), Time.unscaledDeltaTime * response); }
}
