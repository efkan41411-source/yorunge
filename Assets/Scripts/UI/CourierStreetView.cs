using UnityEngine;

public sealed class CourierStreetView : MonoBehaviour
{
    [SerializeField] private RectTransform rider;
    [SerializeField] private RectTransform target;
    [SerializeField] private TMPro.TMP_Text targetLabel;
    [SerializeField] private RectTransform[] dashes;
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform window;
    [SerializeField] private RectTransform perfect;
    [SerializeField] private RectTransform flight;
    [SerializeField] private RectTransform deliveredTarget;
    [SerializeField] private TMPro.TMP_Text deliveredLabel;
    [SerializeField] private TMPro.TMP_Text guide;
    private PaketciSession session;
    private float flightRemaining;
    private Vector2 start, finish;
    public void Bind(PaketciSession model) { session=model;session.Thrown+=Throw; }
    private void OnDestroy(){if(session!=null)session.Thrown-=Throw;}
    private void Throw(float lane,int targetLane,float distance)
    {
        start=new Vector2(LaneX(lane),0.17f);finish=new Vector2(LaneX(targetLane),0.85f-(1-distance)*0.67f);
        deliveredLabel.text=targetLabel.text;
        Place(deliveredTarget,finish.x,finish.y);deliveredTarget.gameObject.SetActive(true);
        flightRemaining=0.32f;flight.gameObject.SetActive(true);
    }
    private static float LaneX(float lane)=>0.2f+lane*0.3f;
    private static void Place(RectTransform rect,float x,float y) { rect.anchorMin=rect.anchorMax=new Vector2(x,y);rect.anchoredPosition=Vector2.zero; }
    private void Update()
    {
        if(session==null||session.Style!=GameplayStyle.Courier)return;
        var courier=session.Courier;
        Place(rider,LaneX(courier.LanePosition),0.16f);
        rider.localRotation=Quaternion.Euler(0,0,(courier.Lane-courier.LanePosition)*-8f);
        Place(target,LaneX(courier.TargetLane),0.85f-(1-courier.Distance)*0.67f);
        string[] shops={"FIRIN","ÇİÇEKÇİ","MARKET","KİTAPÇI","KAFE","MANAV"};string name=shops[session.Deliveries%shops.Length]+"\n"+(12+session.Deliveries*3)+" NUMARA";
        if(targetLabel.text!=name)targetLabel.text=name;
        bool ready=courier.Distance>=session.WindowStart&&courier.Distance<=session.WindowEnd;
        target.localScale=Vector3.one*(ready?1.06f+Mathf.Sin(Time.unscaledTime*8)*0.03f:1);
        for(int i=0;i<dashes.Length;i++)Place(dashes[i],i<6?0.35f:0.65f,0.025f+((i%6)*0.12f+courier.Scroll*0.12f)%0.70f);
        Place(marker,1-Mathf.Clamp01(courier.Distance),0.5f);
        window.anchorMin=new Vector2(1-session.WindowEnd,0);window.anchorMax=new Vector2(1-session.WindowStart,1);window.offsetMin=window.offsetMax=Vector2.zero;
        float center=1-(session.WindowStart+session.WindowEnd)*0.5f;float half=(session.WindowEnd-session.WindowStart)*session.PerfectFraction*0.5f;
        perfect.anchorMin=new Vector2(center-half,0);perfect.anchorMax=new Vector2(center+half,1);perfect.offsetMin=perfect.offsetMax=Vector2.zero;
        string text=courier.Lane!=courier.TargetLane?"Hedef şeridine geç":Mathf.Abs(courier.Lane-courier.LanePosition)>0.08f?"Şerit değiştiriliyor…":ready?"ŞİMDİ FIRLAT!":"Yeşil alanı bekle";
        if(guide.text!=text)guide.text=text;
        if(flightRemaining>0){flightRemaining-=Time.unscaledDeltaTime;float t=1-Mathf.Clamp01(flightRemaining/0.32f);Vector2 p=Vector2.Lerp(start,finish,t);p.y+=Mathf.Sin(t*Mathf.PI)*0.15f;Place(flight,p.x,p.y);flight.localRotation=Quaternion.Euler(0,0,t*150);if(flightRemaining<=0){flight.gameObject.SetActive(false);deliveredTarget.gameObject.SetActive(false);}}
    }
}
