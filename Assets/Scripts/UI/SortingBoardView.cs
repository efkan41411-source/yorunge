using UnityEngine;
using System;

public sealed class SortingBoardView : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[] packages;
    [SerializeField] private UnityEngine.UI.Image[] packageIcons;
    [SerializeField] private TMPro.TMP_Text[] packageLabels;
    [SerializeField] private UnityEngine.UI.Image[] orders;
    [SerializeField] private UnityEngine.UI.Image[] recipeIcons;
    [SerializeField] private TMPro.TMP_Text[] orderLabels;
    [SerializeField] private UnityEngine.UI.Image[] trayIcons;
    [SerializeField] private TMPro.TMP_Text trayLabel;
    [SerializeField] private UnityEngine.UI.Image flight;
    private PaketciSession session;
    private PaketciTheme theme;
    private float flightTime;
    private Vector3 from, to;
    public void Bind(PaketciSession model, PaketciTheme palette)
    {
        session = model; theme = palette;
        for(int i=0;i<packages.Length;i++){int index=i;packages[i].onClick.AddListener(()=>Pick(index));}
        session.Changed += Refresh; Refresh();
    }
    private void OnDestroy() { if(session!=null)session.Changed-=Refresh; }
    private void Pick(int index)
    {
        if(!session.CanPick(index))return;
        from=packageIcons[index].transform.position;to=trayIcons[session.Sorting.Tray.Count].transform.position;
        flight.sprite=theme.Icon(session.Sorting.Packages[index].Color<0?6:session.Sorting.Packages[index].Color);
        flight.color=theme.PackageColor(session.Sorting.Packages[index].Color); flightTime=0.23f;
        flight.gameObject.SetActive(true);session.Pick(index);
    }
    public void Refresh()
    {
        if(session==null||session.Style!=GameplayStyle.Sorting)return;
        for(int i=0;i<packages.Length;i++)
        {
            bool exists=i<session.Sorting.Packages.Count;packages[i].gameObject.SetActive(exists);if(!exists)continue;
            var package=session.Sorting.Packages[i];bool available=session.Sorting.Available(i);
            packages[i].interactable=available && session.State==GameState.Playing;
            packageIcons[i].sprite=theme.Icon(package.Color<0?6:package.Color);
            packageIcons[i].color=theme.PackageColor(package.Color);packageIcons[i].gameObject.SetActive(!package.Used);
            packageLabels[i].text=package.Used?"Teslim edildi":PaketciTheme.ColorName(package.Color);
            packageLabels[i].color=available?theme.Cream:theme.Muted;
            packages[i].GetComponent<UnityEngine.UI.Image>().color=available?theme.Surface:new Color(0.10f,0.16f,0.23f);
        }
        for(int i=0;i<3;i++)
        {
            if(i>=session.Sorting.Orders.Count)continue;
            var order=session.Sorting.Orders[i];orders[i].color=order.Completed?new Color(0.18f,0.38f,0.33f):theme.Cream;
            string[] shops={"Fırıncı Nermin","Çiçekçi Lale","Oyuncakçı Ali"};orderLabels[i].text=order.Completed?shops[i]+" • Teslim edildi":shops[i];
            orderLabels[i].color=order.Completed?theme.Cream:theme.Navy;
            for(int j=0;j<3;j++){int color=order.Recipe[j];var icon=recipeIcons[i*3+j];icon.sprite=theme.Icon(color);icon.color=theme.PackageColor(color);}
            trayIcons[i].gameObject.SetActive(i<session.Sorting.Tray.Count);
            if(i<session.Sorting.Tray.Count){int c=session.Sorting.Packages[session.Sorting.Tray[i]].Color;trayIcons[i].sprite=theme.Icon(c<0?6:c);trayIcons[i].color=theme.PackageColor(c);}
        }
        trayLabel.text="AYIRMA TEPSİSİ  •  "+session.Sorting.Tray.Count+" / 3";
    }
    private void Update()
    {
        if(session==null)return;
        if(flightTime>0){flightTime-=Time.unscaledDeltaTime;float t=1-Mathf.Clamp01(flightTime/0.23f);flight.transform.position=Vector3.Lerp(from,to,t);if(flightTime<=0)flight.gameObject.SetActive(false);}
        if(session.Style!=GameplayStyle.Sorting)return;
        for(int i=0;i<packages.Length;i++)if(packages[i].gameObject.activeSelf){bool hint=session.HintIndex==i&&session.HintUntil>Time.unscaledTime;packageIcons[i].transform.localScale=Vector3.one*(hint?1.08f+Mathf.Sin(Time.unscaledTime*7)*0.08f:1);}
    }
}
