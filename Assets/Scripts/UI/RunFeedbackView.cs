using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class RunFeedbackView : MonoBehaviour
{
    private CanvasGroup group;
    private float remaining;
    private Vector3 origin;
    private void Awake(){group=GetComponent<CanvasGroup>();origin=transform.localPosition;group.alpha=0;}
    public void Show(){remaining=1.5f;group.alpha=1;transform.localPosition=origin;}
    private void Update(){if(remaining<=0)return;remaining-=Time.unscaledDeltaTime;group.alpha=Mathf.Clamp01(remaining*2);transform.localPosition=origin+Vector3.up*((1.5f-remaining)*10);}
}
