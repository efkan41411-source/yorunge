using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PaketciUIController : MonoBehaviour
{
    [Serializable] public struct Element { public string Key; public RectTransform Rect; }
    [SerializeField] private Element[] elements;
    [SerializeField] private PaketciTheme theme;
    [SerializeField] private PaketciGameConfig config;
    [SerializeField] private SortingBoardView sorting;
    [SerializeField] private CourierStreetView courier;
    [SerializeField] private TutorialController tutorial;
    private readonly Dictionary<string,RectTransform> refs = new Dictionary<string,RectTransform>();
    private PaketciSession session;
    private GameMode selectedMode;
    private GameplayStyle selectedStyle;
    private int selectedLevel, selectedContract, page, tutorialStep;
    private string modal = "";
    private bool settingsPaused, adWaiting;
    private int displayedSeconds = -1;
    public PaketciSession Session => session;
    public PaketciTheme Theme => theme;
    public event Action<string,bool> Feedback;
    private RectTransform R(string key) => refs[key];
    private TMPro.TMP_Text T(string key) => R(key).GetComponent<TMPro.TMP_Text>();
    private UnityEngine.UI.Button B(string key) => R(key).GetComponent<UnityEngine.UI.Button>();
    private void Text(string key,string text){if(T(key).text!=text)T(key).text=text;}
    private void Click(string key,Action action){B(key).onClick.AddListener(()=>{Feedback?.Invoke("tap",true);action();});}
    private void Awake()
    {
        foreach(var entry in elements)refs.Add(entry.Key,entry.Rect);
        session=new PaketciSession(config,new PaketciSaveRepository(new PlayerPrefsPaketciStore()));
        selectedStyle=session.Save.PreferredStyle;selectedLevel=session.Save.UnlockedLevel;
        sorting.Bind(session,theme);courier.Bind(session);session.Changed+=Refresh;session.Feedback+=OnFeedback;
        Click("Play",StartSelectedRun);Click("Settings",()=>Open("Settings"));Click("Map",()=>{page=(selectedLevel-1)/12;Open("Map");});
        Click("StyleCourier",()=>SelectStyle(GameplayStyle.Courier));Click("StyleSorting",()=>SelectStyle(GameplayStyle.Sorting));
        for(int i=0;i<3;i++){int k=i;Click("Mode"+i,()=>{selectedMode=(GameMode)k;Refresh();});Click("Daily"+i,()=>{selectedContract=k;Refresh();});Click("Lane"+i,()=>session.SetLane(k));}
        Click("Pause",()=>session.Pause());Click("Throw",()=>session.Throw());Click("Hint",()=>session.Hint());Click("Undo",()=>session.Undo());
        Click("Resume",()=>session.Resume());Click("PauseSettings",()=>Open("Settings"));Click("Restart",()=>{modal="";StartSelectedRun();});Click("Menu",ToMenu);
        Click("CloseSettings",Close);Click("CloseMap",Close);
        Click("Sound",()=>{var s=session.Save;s.Settings(!s.Sound,s.Haptics,s.Volume);Refresh();});
        Click("Haptics",()=>{var s=session.Save;s.Settings(s.Sound,!s.Haptics,s.Volume);Refresh();});
        R("Volume").GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener(v=>{var s=session.Save;if(!Mathf.Approximately(s.Volume,v)){s.Settings(s.Sound,s.Haptics,v);Text("VolumeLabel",Mathf.RoundToInt(v*100)+"% ");}});
        Click("Privacy",()=>{if(RewardedAdService.Instance!=null)RewardedAdService.Instance.ShowPrivacyOptions(()=>{});});
        Click("Next",()=>{selectedLevel=Mathf.Min(session.Level+1,session.Save.UnlockedLevel);modal="";StartSelectedRun();});Click("Retry",()=>{modal="";StartSelectedRun();});Click("ResultMenu",ToMenu);Click("Revive",RequestRevive);
        Click("TutorialNext",()=>{tutorialStep++;if(tutorialStep>=2)session.FinishTutorial();else Refresh();});Click("TutorialSkip",()=>session.FinishTutorial());
        for(int i=0;i<12;i++){int k=i;Click("Level"+i,()=>{selectedLevel=page*12+k+1;Close();});}
        Click("PageLeft",()=>{page=Mathf.Max(0,page-1);RefreshMap();});Click("PageRight",()=>{page=Mathf.Min((session.Save.UnlockedLevel-1)/12,page+1);RefreshMap();});
        Refresh();
    }
    private void OnDestroy(){if(session!=null){session.Changed-=Refresh;session.Feedback-=OnFeedback;}}
    private void OnFeedback(string message,bool good){Feedback?.Invoke(message,good);Text("Toast",message);T("Toast").color=good?theme.Mint:theme.Orange;R("Toast").GetComponent<RunFeedbackView>().Show();}
    private void SelectStyle(GameplayStyle style){selectedStyle=style;session.Save.SelectStyle(style);Refresh();}
    private void StartSelectedRun(){tutorialStep=0;modal="";adWaiting=false;session.StartRun(selectedStyle,selectedMode,selectedLevel,selectedContract);}
    private void ToMenu(){modal="";adWaiting=false;selectedLevel=Mathf.Min(Mathf.Max(selectedLevel,session.Level),session.Save.UnlockedLevel);session.Menu();}
    private void Open(string name){settingsPaused=session.State==GameState.Playing;if(settingsPaused)session.Pause();modal=name;Refresh();}
    private void Close(){modal="";if(settingsPaused){settingsPaused=false;session.Resume();}else Refresh();}
    public void Back(){if(adWaiting)return;if(modal!=""){Close();return;}if(session.State==GameState.Playing)session.Pause();else if(session.State==GameState.Paused)session.Resume();else if(session.State==GameState.Tutorial)session.FinishTutorial();else if(session.State!=GameState.Menu)ToMenu();}
    public void Move(int direction){if(modal=="")session.SetLane(session.Courier.Lane+direction);}
    public void Throw(){if(modal=="")session.Throw();}
    private void RequestRevive()
    {
        if(adWaiting||session.ReviveUsed)return;
        var service=RewardedAdService.Instance;
        if(service==null||!service.IsReady){Text("AdStatus","Reklam hazır değil. Ücretsiz yeniden deneyebilirsin.");return;}
        int id=session.RunId;adWaiting=true;Refresh();
        service.Show(()=>{if(session.RunId!=id)return;adWaiting=false;session.Revive(id);},()=>AdClosed(id,"Reklam şu an açılamıyor."),()=>AdClosed(id,"Ek süre için videoyu tamamla."));
    }
    private void AdClosed(int id,string message){if(session.RunId!=id)return;adWaiting=false;Refresh();Text("AdStatus",message);}
    private void Update()
    {
        session.Tick(Time.unscaledDeltaTime);
        int seconds=Mathf.CeilToInt(session.Remaining);
        if(seconds!=displayedSeconds){displayedSeconds=seconds;Text("Timer",session.Timed?seconds+" sn":"Süresiz");T("Timer").color=session.Timed&&seconds<=10?theme.Orange:theme.Cream;}
        if(session.State==GameState.Playing&&session.Style==GameplayStyle.Courier){var c=session.Courier;B("Throw").GetComponent<UnityEngine.UI.Image>().color=c.Lane==c.TargetLane&&c.Distance>=session.WindowStart&&c.Distance<=session.WindowEnd?theme.Mint:theme.Orange;}
    }
    private void OnApplicationPause(bool paused){if(paused&&session!=null){session.Pause();session.Save.Flush();}}
    public void Refresh()
    {
        if(session==null)return;
        bool menu=session.State==GameState.Menu;
        R("MenuScreen").gameObject.SetActive(menu);R("GameScreen").gameObject.SetActive(!menu);
        R("SortingScreen").gameObject.SetActive(!menu&&session.Style==GameplayStyle.Sorting);R("CourierScreen").gameObject.SetActive(!menu&&session.Style==GameplayStyle.Courier);
        foreach(string p in new[]{"Settings","Map","Pause","Result","Tutorial"})R(p+"Panel").gameObject.SetActive(false);
        string active=modal!=""?modal:session.State==GameState.Paused?"Pause":session.State==GameState.Completed||session.State==GameState.Failed?"Result":session.State==GameState.Tutorial?"Tutorial":"";
        R("Modal").gameObject.SetActive(active!="");if(active!="")R(active+"Panel").gameObject.SetActive(true);
        R("MenuScreen").GetComponent<CanvasGroup>().interactable=active=="";R("GameScreen").GetComponent<CanvasGroup>().interactable=active=="";
        Text("Wallet",session.Save.Coins+" jeton");Text("MenuDistrict",PaketciGameConfig.District(selectedLevel));
        Text("LevelSummary","BÖLÜM "+selectedLevel.ToString("00")+"  •  "+session.Save.Stars+" yıldız");
        Text("PlayLabel",selectedStyle==GameplayStyle.Courier?"TESLİMATA BAŞLA  →":"PAKETLERİ EŞLEŞTİR  →");
        Text("ModeSummary",selectedMode==GameMode.Story?(selectedStyle==GameplayStyle.Courier?"Doğru şerit. Doğru zaman. Mutlu mahalle.":"Üç paketi eşleştir. Mahalleni renklendir."):selectedMode==GameMode.Rush?"75 saniye • Seri teslimatlarla rekorunu geç!":"Her gün yenilenen üç özel rota.");
        R("DailyCards").gameObject.SetActive(selectedMode==GameMode.Daily);R("LevelCard").gameObject.SetActive(selectedMode!=GameMode.Daily);
        for(int i=0;i<3;i++){B("Mode"+i).GetComponent<UnityEngine.UI.Image>().color=(int)selectedMode==i?theme.Orange:theme.Surface;T("ModeLabel"+i).color=(int)selectedMode==i?theme.Navy:theme.Cream;T("ModeSubtitle"+i).color=(int)selectedMode==i?theme.Navy:theme.Muted;B("Daily"+i).GetComponent<UnityEngine.UI.Image>().color=selectedContract==i?theme.Mint:theme.Surface;Text("DailyLabel"+i,(session.Save.DailyComplete(i)?"TAMAM":"ROTA "+(i+1))+"\n"+PaketciGameConfig.DailyDuration(i)+" sn");}
        B("StyleCourier").GetComponent<UnityEngine.UI.Image>().color=selectedStyle==GameplayStyle.Courier?theme.Cream:theme.Surface;T("StyleCourierLabel").color=selectedStyle==GameplayStyle.Courier?theme.Navy:theme.Cream;
        B("StyleSorting").GetComponent<UnityEngine.UI.Image>().color=selectedStyle==GameplayStyle.Sorting?theme.Cream:theme.Surface;T("StyleSortingLabel").color=selectedStyle==GameplayStyle.Sorting?theme.Navy:theme.Cream;
        Text("GameTitle",session.Mode==GameMode.Story?PaketciGameConfig.District(session.Level):session.Mode==GameMode.Rush?"Ekspres Teslimat":PaketciGameConfig.Scenario(session.Contract));
        Text("Progress",session.Mode==GameMode.Rush?"TESLİMAT "+session.Deliveries+" • REKOR "+session.Save.Best(session.Style):"TESLİMAT "+session.Deliveries+" / "+session.Goal);
        Text("Combo",session.Combo>1?"SERİ x"+session.Combo:"BÖLÜM "+session.Level.ToString("00"));Text("Message",session.Message);Text("HintLabel",session.HintUsed?"İPUCU • 5 jeton":"İPUCU • Ücretsiz");
        B("Undo").interactable=session.Sorting.Tray.Count>0;for(int i=0;i<3;i++){B("Lane"+i).GetComponent<UnityEngine.UI.Image>().color=session.Courier.Lane==i?theme.Cream:theme.Surface;T("LaneLabel"+i).color=session.Courier.Lane==i?theme.Navy:theme.Cream;}
        if(active=="Settings"){var s=session.Save;Text("SoundLabel",s.Sound?"AÇIK":"KAPALI");Text("HapticsLabel",s.Haptics?"AÇIK":"KAPALI");R("Volume").GetComponent<UnityEngine.UI.Slider>().SetValueWithoutNotify(s.Volume);Text("VolumeLabel",Mathf.RoundToInt(s.Volume*100)+"%");R("Privacy").gameObject.SetActive(RewardedAdService.Instance!=null&&RewardedAdService.Instance.PrivacyOptionsRequired);}
        if(active=="Map")RefreshMap();
        if(active=="Tutorial"){Text("TutorialTitle",tutorialStep==0?"Mahallene hoş geldin!":"Harika bir teslimata hazır mısın?");Text("TutorialBody",session.Style==GameplayStyle.Sorting?(tutorialStep==0?"Siparişlerdeki renk ve simgeleri incele.\nBir sipariş için üç uygun paketi seç.":"Joker eksik rengi tamamlar.\nGeri al ile seçimini değiştir; ilk ipucu ücretsiz."):(tutorialStep==0?"SOL, ORTA ve SAĞ düğmeleriyle\nhedef adresin şeridine geç.":"İşaret yeşil alandayken FIRLAT!\nOrtadaki altın alan MÜKEMMEL teslimattır."));Text("TutorialNextLabel",tutorialStep==0?"ANLADIM  →":"HADİ OYNAYALIM  →");}
        if(active=="Result")
        {
            bool completed=session.State==GameState.Completed;Text("ResultTitle",session.Mode==GameMode.Rush?"Tur tamamlandı!":completed?"Güzel iş, paketçi!":"Bir kez daha dene!");
            var resultIcon=R("ResultIcon").GetComponent<UnityEngine.UI.Image>();resultIcon.sprite=theme.Icon(completed?10:6);resultIcon.color=completed?theme.Mint:theme.Orange;
            Text("ResultBody",session.Deliveries+" teslimat  •  "+session.Mistakes+" hata\n"+(session.Style==GameplayStyle.Courier?session.PerfectCount+" mükemmel teslimat":"Mahalle seninle daha renkli."));
            Text("ResultReward","+"+session.EarnedCoins+" jeton");Text("ResultStars",session.Mode==GameMode.Rush?"REKOR  "+session.Save.Best(session.Style):session.StarsAwarded+" / 3 YILDIZ");
            B("Next").gameObject.SetActive(completed&&session.Mode==GameMode.Story);B("Retry").gameObject.SetActive(!completed||session.Mode!=GameMode.Story);
            B("Revive").gameObject.SetActive(!session.ReviveUsed&&(!completed||session.Mode==GameMode.Rush));B("Revive").interactable=!adWaiting;B("Retry").interactable=!adWaiting;B("ResultMenu").interactable=!adWaiting;
            Text("AdStatus",adWaiting?"Reklam açılıyor…":"İstersen video izleyerek +30 saniye kazan.");
        }
        if(active=="Tutorial")tutorial.Configure(session.Style,tutorialStep);
        sorting.Refresh();
    }
    private void RefreshMap()
    {
        Text("MapTitle","Mahalle haritası • "+(page+1));
        for(int i=0;i<12;i++){int level=page*12+i+1;bool unlocked=level<=session.Save.UnlockedLevel;B("Level"+i).interactable=unlocked;Text("LevelLabel"+i,unlocked?level.ToString("00")+"\n"+session.Save.LevelStars(level)+" / 3":"KİLİTLİ");B("Level"+i).GetComponent<UnityEngine.UI.Image>().color=level==selectedLevel?theme.Orange:theme.Surface;}
        B("PageLeft").interactable=page>0;B("PageRight").interactable=page<(session.Save.UnlockedLevel-1)/12;
    }
}
