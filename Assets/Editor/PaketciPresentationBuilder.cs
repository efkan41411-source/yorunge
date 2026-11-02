using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class PaketciPresentationBuilder
{
    private static PaketciTheme theme;
    private static readonly Dictionary<string,RectTransform> refs=new Dictionary<string,RectTransform>();
    private static RectTransform Box(Transform parent,string key,float x,float y,float w,float h)
    {
        var obj=new GameObject(key,typeof(RectTransform));var r=(RectTransform)obj.transform;r.SetParent(parent,false);r.anchorMin=new Vector2(x,1-y-h);r.anchorMax=new Vector2(x+w,1-y);r.offsetMin=r.offsetMax=Vector2.zero;if(!refs.ContainsKey(key))refs.Add(key,r);return r;
    }
    private static RectTransform Point(Transform parent,string key,float x,float y,float w,float h)
    {
        var r=Box(parent,key,0,0,0,0);r.anchorMin=r.anchorMax=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=Vector2.zero;return r;
    }
    private static UnityEngine.UI.Image Image(RectTransform r,Color color,Sprite sprite=null,bool sliced=true)
    {
        var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();im.color=color;im.sprite=sprite==null?theme.Panel:sprite;im.type=sliced?UnityEngine.UI.Image.Type.Sliced:UnityEngine.UI.Image.Type.Simple;im.raycastTarget=false;
        if(r.name.StartsWith("Dash")||r.name=="Background"||r.name=="Modal"||r.name=="Road"||r.name=="SidewalkL"||r.name=="SidewalkR"||r.name=="TimingWindow"||r.name=="PerfectWindow"){im.sprite=null;im.type=UnityEngine.UI.Image.Type.Simple;}
        return im;
    }
    private static RectTransform Panel(Transform parent,string key,float x,float y,float w,float h,Color c){var r=Box(parent,key,x,y,w,h);Image(r,c);return r;}
    private static TMPro.TMP_Text Label(Transform parent,string key,string text,float x,float y,float w,float h,float size,Color color,TMPro.TextAlignmentOptions align=TMPro.TextAlignmentOptions.MidlineLeft)
    {
        var r=Box(parent,key,x,y,w,h);var t=r.gameObject.AddComponent<TMPro.TextMeshProUGUI>();t.font=theme.Font;t.text=text;t.fontSize=size;t.color=color;t.alignment=align;t.enableAutoSizing=false;t.raycastTarget=false;t.overflowMode=TMPro.TextOverflowModes.Ellipsis;return t;
    }
    private static RectTransform Button(Transform parent,string key,string label,float x,float y,float w,float h,Color color,float size=13,string labelKey=null)
    {
        var r=Panel(parent,key,x,y,w,h,color);var im=r.GetComponent<UnityEngine.UI.Image>();im.raycastTarget=true;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=im;var cb=b.colors;cb.normalColor=Color.white;cb.highlightedColor=new Color(1.06f,1.06f,1.06f);cb.pressedColor=new Color(0.86f,0.86f,0.86f);cb.disabledColor=new Color(0.6f,0.6f,0.6f,0.7f);cb.fadeDuration=0.12f;b.colors=cb;b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};r.gameObject.AddComponent<ButtonFeedback>();var shadow=r.gameObject.AddComponent<UnityEngine.UI.Shadow>();shadow.effectColor=new Color(0,0,0,0.2f);shadow.effectDistance=new Vector2(0,-3);
        Label(r,labelKey??key+"Label",label,0.06f,0.04f,0.88f,0.92f,size,color==theme.Orange||color==theme.Mint||color==theme.Cream?theme.Navy:theme.Cream,TMPro.TextAlignmentOptions.Center);return r;
    }
    private static UnityEngine.UI.Image Icon(Transform parent,string key,Sprite sprite,float x,float y,float w,float h,Color color){var im=Image(Box(parent,key,x,y,w,h),color,sprite,false);im.preserveAspect=true;return im;}
    private static void Field(Object owner,string name,Object value){var so=new SerializedObject(owner);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    private static void Array(Object owner,string name,List<Object> values){var so=new SerializedObject(owner);var p=so.FindProperty(name);p.arraySize=values.Count;for(int i=0;i<values.Count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
    public static GameObject Build()
    {
        refs.Clear();theme=AssetDatabase.LoadAssetAtPath<PaketciTheme>("Assets/UI/PaketciTheme.asset");
        var root=new GameObject("PaketciUI",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;
        var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(390,780);scaler.matchWidthOrHeight=0.5f;
        Panel(root.transform,"Background",0,0,1,1,theme.Navy);
        var safe=Box(root.transform,"SafeArea",0,0,1,1);safe.gameObject.AddComponent<SafeAreaLayout>();
        var menu=Box(safe,"MenuScreen",0,0,1,1);menu.gameObject.AddComponent<CanvasGroup>();
        Label(menu,"Brand","YÖRÜNGE  /  KÖRFEZ ROTASI",0.055f,0.024f,0.52f,0.04f,10,theme.Muted);
        var wallet=Panel(menu,"WalletPill",0.60f,0.018f,0.235f,0.055f,theme.Surface);Icon(wallet,"CoinIcon",theme.Icon(9),0.07f,0.14f,0.20f,0.72f,theme.Orange);Label(wallet,"Wallet","0 jeton",0.30f,0,0.67f,1,11,theme.Cream);
        var settings=Button(menu,"Settings","",0.85f,0.018f,0.105f,0.06f,theme.Surface);Icon(settings,"SettingsIcon",theme.Icon(7),0.22f,0.22f,0.56f,0.56f,theme.Cream);
        var hero=Panel(menu,"Hero",0.05f,0.095f,0.90f,0.30f,new Color(0.19f,0.30f,0.39f));hero.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=true;
        Icon(hero,"Town",theme.Town,0,0.30f,1,0.70f,Color.white).preserveAspect=false;
        Label(hero,"HeroEyebrow","MAHALLENİN YENİ KAHRAMANI",0.055f,0.04f,0.90f,0.10f,9,theme.Mint);
        Label(hero,"HeroTitle","ŞEHRİN\nPAKETÇİSİ",0.055f,0.16f,0.66f,0.42f,30,theme.Cream);
        Icon(hero,"Mascot",theme.Mascot,0.63f,0.24f,0.35f,0.58f,Color.white);
        var chip=Panel(hero,"DistrictPill",0.055f,0.80f,0.59f,0.14f,theme.Navy);Label(chip,"MenuDistrict","İstasyon Caddesi",0.06f,0,0.9f,1,11,theme.Cream);
        Label(menu,"StyleCaption","BUGÜN NASIL OYNAYALIM?",0.06f,0.417f,0.9f,0.025f,10,theme.Muted);
        Button(menu,"StyleCourier","Kurye Aksiyonu",0.05f,0.452f,0.44f,0.069f,theme.Cream,13);
        Button(menu,"StyleSorting","Paket Eşleştirme",0.51f,0.452f,0.44f,0.069f,theme.Surface,12);
        string[] modes={"MAHALLE","EKSPRES","GÜNLÜK"};string[] sub={"Senin tempon","75 sn • Rekor","3 özel rota"};
        for(int i=0;i<3;i++){var b=Button(menu,"Mode"+i,modes[i],0.05f+i*0.307f,0.552f,0.285f,0.12f,i==0?theme.Orange:theme.Surface,12,"ModeLabel"+i);refs["ModeLabel"+i].anchorMin=new Vector2(0.05f,0.40f);refs["ModeLabel"+i].anchorMax=new Vector2(0.95f,0.96f);Label(b,"ModeSubtitle"+i,sub[i],0.04f,0.62f,0.92f,0.26f,9,theme.Muted,TMPro.TextAlignmentOptions.Center);}
        Label(menu,"ModeSummary","Doğru şerit. Doğru zaman. Mutlu mahalle.",0.065f,0.686f,0.87f,0.05f,12,theme.Muted,TMPro.TextAlignmentOptions.Center);
        var levelCard=Panel(menu,"LevelCard",0.05f,0.751f,0.90f,0.097f,theme.Surface);Label(levelCard,"LevelSummary","BÖLÜM 01 • 0 yıldız",0.06f,0.1f,0.54f,0.4f,11,theme.Cream);Label(levelCard,"LevelHint","Yeni sokaklar seni bekliyor",0.06f,0.54f,0.56f,0.32f,10,theme.Muted);Button(levelCard,"Map","HARİTA",0.67f,0.14f,0.29f,0.74f,theme.Navy,11);
        var daily=Box(menu,"DailyCards",0.05f,0.751f,0.90f,0.097f);for(int i=0;i<3;i++)Button(daily,"Daily"+i,"ROTA "+(i+1)+"\n90 sn",i*0.34f,0,0.32f,1,i==0?theme.Mint:theme.Surface,11,"DailyLabel"+i);daily.gameObject.SetActive(false);
        Button(menu,"Play","TESLİMATA BAŞLA  →",0.05f,0.875f,0.90f,0.085f,theme.Orange,17);Label(menu,"Footer","İlerlemen kaydedilir. Mahallen hep burada.",0.05f,0.969f,0.9f,0.023f,9,theme.Muted,TMPro.TextAlignmentOptions.Center);
        var game=Box(safe,"GameScreen",0,0,1,1);game.gameObject.AddComponent<CanvasGroup>();
        Panel(game,"Header",0.045f,0.018f,0.91f,0.094f,theme.Surface);Label(game,"GameTitle","İstasyon Caddesi",0.07f,0.027f,0.59f,0.032f,14,theme.Cream);Label(game,"Combo","BÖLÜM 01",0.07f,0.066f,0.45f,0.026f,10,theme.Mint);Label(game,"Timer","65 sn",0.65f,0.039f,0.18f,0.04f,16,theme.Cream,TMPro.TextAlignmentOptions.Center);
        var pause=Button(game,"Pause","",0.843f,0.026f,0.10f,0.066f,theme.Navy);Icon(pause,"PauseIcon",theme.Icon(8),0.18f,0.18f,0.64f,0.64f,theme.Cream);Label(game,"Progress","TESLİMAT 0 / 3",0.055f,0.127f,0.9f,0.028f,12,theme.Muted);
        var sort=Box(game,"SortingScreen",0,0.171f,1,0.817f);var sorting=sort.gameObject.AddComponent<SortingBoardView>();
        var packageButtons=new List<Object>();var packageIcons=new List<Object>();var packageLabels=new List<Object>();var orderImages=new List<Object>();var orderLabels=new List<Object>();var recipes=new List<Object>();var trays=new List<Object>();
        for(int i=0;i<3;i++){var o=Panel(sort,"Order"+i,0.05f,i*0.132f,0.90f,0.12f,theme.Cream);orderImages.Add(o.GetComponent<UnityEngine.UI.Image>());orderLabels.Add(Label(o,"OrderLabel"+i,"Fırıncı Nermin",0.055f,0.08f,0.90f,0.28f,13,theme.Navy));for(int j=0;j<3;j++){var tile=Panel(o,"RecipeTile"+(i*3+j),0.05f+j*0.305f,0.43f,0.285f,0.49f,new Color(0.93f,0.89f,0.8f));recipes.Add(Icon(tile,"RecipeIcon"+(i*3+j),theme.Icon(j),0.05f,0.06f,0.9f,0.88f,theme.PackageColor(j)));}}
        var tray=Panel(sort,"Tray",0.05f,0.417f,0.90f,0.16f,theme.Surface);Field(sorting,"trayLabel",Label(tray,"TrayLabel","AYIRMA TEPSİSİ • 0 / 3",0.055f,0.065f,0.9f,0.25f,10,theme.Muted));for(int i=0;i<3;i++){var slot=Panel(tray,"Slot"+i,0.06f+i*0.30f,0.37f,0.28f,0.52f,theme.Navy);trays.Add(Icon(slot,"TrayIcon"+i,theme.Icon(i),0.08f,0.05f,0.84f,0.90f,theme.PackageColor(i)));}
        Button(sort,"Hint","İPUCU • Ücretsiz",0.05f,0.597f,0.54f,0.082f,theme.Mint,11);Button(sort,"Undo","GERİ AL",0.61f,0.597f,0.34f,0.082f,theme.Surface,12);
        var scroll=Box(sort,"PackageScroll",0.05f,0.70f,0.9f,0.295f);var sr=scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();sr.horizontal=false;sr.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
        var viewport=Box(scroll,"Viewport",0,0,1,1);Image(viewport,new Color(1,1,1,0.01f)).raycastTarget=true;viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic=false;
        var content=Box(viewport,"PackageContent",0,0,1,0);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(0.5f,1);content.sizeDelta=new Vector2(0,268);
        var vlg=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();vlg.spacing=8;vlg.childControlWidth=true;vlg.childControlHeight=true;vlg.childForceExpandHeight=false;var fitter=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fitter.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;sr.content=content;sr.viewport=viewport;
        for(int row=0;row<4;row++){var rowRect=Box(content,"PackageRow"+row,0,0,1,0);var le=rowRect.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();le.preferredHeight=60;var hl=rowRect.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();hl.spacing=8;hl.childControlWidth=hl.childControlHeight=true;hl.childForceExpandWidth=true;for(int col=0;col<3;col++){int i=row*3+col;var b=Button(rowRect,"Package"+i,"",0,0,1,1,theme.Surface);b.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().flexibleWidth=1;packageButtons.Add(b.GetComponent<UnityEngine.UI.Button>());packageIcons.Add(Icon(b,"PackageIcon"+i,theme.Icon(i%6),0.08f,0.06f,0.84f,0.56f,theme.PackageColor(i%6)));packageLabels.Add(Label(b,"PackageLabel"+i,"Kırmızı",0.025f,0.64f,0.95f,0.31f,10,theme.Cream,TMPro.TextAlignmentOptions.Center));}}
        var flight=Icon(sort,"SortingFlight",theme.Icon(0),0.45f,0.5f,0.1f,0.05f,theme.Orange);flight.gameObject.SetActive(false);Field(sorting,"flight",flight);Array(sorting,"packages",packageButtons);Array(sorting,"packageIcons",packageIcons);Array(sorting,"packageLabels",packageLabels);Array(sorting,"orders",orderImages);Array(sorting,"orderLabels",orderLabels);Array(sorting,"recipeIcons",recipes);Array(sorting,"trayIcons",trays);
        var cour=Box(game,"CourierScreen",0,0.17f,1,0.817f);var cv=cour.gameObject.AddComponent<CourierStreetView>();var street=Panel(cour,"Street",0.05f,0,0.9f,0.65f,new Color(0.21f,0.32f,0.37f));street.gameObject.AddComponent<UnityEngine.UI.Mask>();Icon(street,"StreetTown",theme.Town,0,0,1,0.27f,Color.white).preserveAspect=false;Panel(street,"SidewalkL",0,0.24f,0.06f,0.76f,new Color(0.76f,0.65f,0.51f));Panel(street,"SidewalkR",0.94f,0.24f,0.06f,0.76f,new Color(0.76f,0.65f,0.51f));Panel(street,"Road",0.06f,0.24f,0.88f,0.76f,new Color(0.20f,0.24f,0.29f));
        var dashes=new List<Object>();for(int i=0;i<12;i++){var d=Point(street,"Dash"+i,i<6?0.35f:0.65f,0.2f,3,22);Image(d,new Color(0.82f,0.77f,0.65f));dashes.Add(d);}
        var target=Point(street,"Target",0.5f,0.85f,80,57);Image(target,theme.Cream);Icon(target,"Pin",theme.Icon(4),0.36f,0.05f,0.28f,0.3f,theme.Orange);Field(cv,"targetLabel",Label(target,"TargetLabel","FIRIN\n12 NUMARA",0.04f,0.37f,0.92f,0.57f,9,theme.Navy,TMPro.TextAlignmentOptions.Center));
        var rider=Point(street,"Rider",0.5f,0.16f,64,88);var ri=Image(rider,Color.white,theme.Courier,false);ri.preserveAspect=true;
        var parcel=Point(street,"ParcelFlight",0.5f,0.3f,24,24);Image(parcel,theme.Orange,theme.Icon(6),false);parcel.gameObject.SetActive(false);
        Field(cv,"rider",rider);Field(cv,"target",target);Field(cv,"flight",parcel);Array(cv,"dashes",dashes);
        Field(cv,"guide",Label(cour,"Guide","Yeşil alanı bekle",0.05f,0.675f,0.9f,0.045f,13,theme.Cream,TMPro.TextAlignmentOptions.Center));
        var timing=Panel(cour,"Timing",0.12f,0.736f,0.76f,0.012f,theme.Surface);Field(cv,"window",Panel(timing,"TimingWindow",0.58f,0,0.24f,1,theme.Mint));Field(cv,"perfect",Panel(timing,"PerfectWindow",0.66f,0,0.08f,1,theme.Orange));var mark=Point(timing,"TimingMarker",0.5f,0.5f,12,16);Image(mark,theme.Cream);Field(cv,"marker",mark);
        for(int i=0;i<3;i++)Button(cour,"Lane"+i,i==0?"SOL":i==1?"ORTA":"SAĞ",0.05f+i*0.307f,0.78f,0.285f,0.09f,theme.Surface,13,"LaneLabel"+i);
        Button(cour,"Throw","PAKETİ FIRLAT",0.05f,0.90f,0.9f,0.097f,theme.Orange,17);
        Label(game,"Message","",0.07f,0.152f,0.86f,0.018f,9,theme.Muted,TMPro.TextAlignmentOptions.Center);
        var modal=Panel(safe,"Modal",0,0,1,1,new Color(0.025f,0.04f,0.065f,0.93f));modal.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
        var pausePanel=Panel(modal,"PausePanel",0.055f,0.23f,0.89f,0.53f,theme.Surface);Label(pausePanel,"PauseTitle","Kısa bir mola",0.08f,0.07f,0.84f,0.12f,26,theme.Cream,TMPro.TextAlignmentOptions.Center);Label(pausePanel,"PauseSubtitle","Mahallen seni bekliyor.",0.08f,0.20f,0.84f,0.08f,12,theme.Muted,TMPro.TextAlignmentOptions.Center);
        Button(pausePanel,"Resume","DEVAM ET",0.08f,0.34f,0.84f,0.14f,theme.Orange,16);Button(pausePanel,"PauseSettings","AYARLAR",0.08f,0.51f,0.84f,0.12f,theme.Navy);Button(pausePanel,"Restart","YENİDEN BAŞLAT",0.08f,0.66f,0.84f,0.12f,theme.Navy);Button(pausePanel,"Menu","ANA MENÜ",0.08f,0.81f,0.84f,0.12f,theme.Navy);
        var sp=Panel(modal,"SettingsPanel",0.055f,0.21f,0.89f,0.58f,theme.Surface);Label(sp,"SettingsTitle","Sana göre ayarla",0.08f,0.05f,0.84f,0.12f,24,theme.Cream,TMPro.TextAlignmentOptions.Center);
        Label(sp,"SoundCaption","Ses efektleri",0.08f,0.22f,0.50f,0.1f,15,theme.Cream);Button(sp,"Sound","AÇIK",0.65f,0.22f,0.27f,0.11f,theme.Mint,12);
        Label(sp,"VolumeCaption","Ses seviyesi",0.08f,0.37f,0.55f,0.08f,13,theme.Muted);Label(sp,"VolumeLabel","65%",0.70f,0.37f,0.22f,0.08f,12,theme.Cream,TMPro.TextAlignmentOptions.Center);
        var slider=Box(sp,"Volume",0.08f,0.46f,0.84f,0.04f);var sl=slider.gameObject.AddComponent<UnityEngine.UI.Slider>();Image(slider,theme.Navy);var fill=Panel(slider,"VolumeFill",0,0,1,1,theme.Orange);var handle=Point(slider,"VolumeHandle",0.65f,0.5f,20,20);Image(handle,theme.Cream).raycastTarget=true;sl.fillRect=fill;sl.handleRect=handle;sl.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>();sl.minValue=0;sl.maxValue=1;sl.value=0.65f;
        Label(sp,"HapticsCaption","Titreşim",0.08f,0.56f,0.50f,0.1f,15,theme.Cream);Button(sp,"Haptics","AÇIK",0.65f,0.56f,0.27f,0.11f,theme.Mint,12);Button(sp,"Privacy","GİZLİLİK SEÇENEKLERİ",0.08f,0.71f,0.84f,0.09f,theme.Navy,11);Button(sp,"CloseSettings","TAMAM",0.08f,0.84f,0.84f,0.11f,theme.Orange,15);
        var mp=Panel(modal,"MapPanel",0.04f,0.17f,0.92f,0.66f,theme.Surface);Label(mp,"MapTitle","Mahalle haritası",0.08f,0.05f,0.84f,0.09f,21,theme.Cream,TMPro.TextAlignmentOptions.Center);
        for(int i=0;i<12;i++)Button(mp,"Level"+i,(i+1).ToString("00")+"\n0 / 3",0.06f+(i%4)*0.235f,0.20f+(i/4)*0.19f,0.175f,0.15f,theme.Navy,12,"LevelLabel"+i);
        Button(mp,"PageLeft","←",0.06f,0.79f,0.16f,0.12f,theme.Navy);Button(mp,"PageRight","→",0.78f,0.79f,0.16f,0.12f,theme.Navy);Button(mp,"CloseMap","KAPAT",0.26f,0.79f,0.48f,0.12f,theme.Orange);
        var rp=Panel(modal,"ResultPanel",0.055f,0.16f,0.89f,0.69f,theme.Surface);Icon(rp,"ResultIcon",theme.Icon(10),0.38f,0.02f,0.24f,0.10f,theme.Mint);Label(rp,"ResultTitle","Güzel iş, paketçi!",0.06f,0.13f,0.88f,0.08f,22,theme.Cream,TMPro.TextAlignmentOptions.Center);Label(rp,"ResultBody","3 teslimat • 0 hata",0.07f,0.23f,0.86f,0.10f,13,theme.Muted,TMPro.TextAlignmentOptions.Center);Label(rp,"ResultStars","3 / 3 YILDIZ",0.06f,0.35f,0.88f,0.07f,18,theme.Orange,TMPro.TextAlignmentOptions.Center);Label(rp,"ResultReward","+21 jeton",0.06f,0.43f,0.88f,0.07f,23,theme.Mint,TMPro.TextAlignmentOptions.Center);Button(rp,"Revive","VİDEO İZLE • +30 SANİYE",0.08f,0.55f,0.84f,0.09f,theme.Navy,12);Label(rp,"AdStatus","",0.06f,0.65f,0.88f,0.045f,9,theme.Muted,TMPro.TextAlignmentOptions.Center);Button(rp,"Next","SONRAKİ TESLİMAT →",0.08f,0.72f,0.84f,0.10f,theme.Orange,14);Button(rp,"Retry","YENİDEN DENE",0.08f,0.72f,0.84f,0.10f,theme.Orange,14);Button(rp,"ResultMenu","ANA MENÜ",0.08f,0.86f,0.84f,0.09f,theme.Navy,12);
        var tp=Panel(modal,"TutorialPanel",0.055f,0.19f,0.89f,0.61f,theme.Surface);Icon(tp,"TutorialMascot",theme.Mascot,0.35f,0.02f,0.30f,0.24f,Color.white);Label(tp,"TutorialTitle","Mahallene hoş geldin!",0.06f,0.29f,0.88f,0.12f,22,theme.Cream,TMPro.TextAlignmentOptions.Center);Label(tp,"TutorialBody","",0.08f,0.44f,0.84f,0.19f,14,theme.Muted,TMPro.TextAlignmentOptions.Center);Button(tp,"TutorialNext","ANLADIM →",0.08f,0.70f,0.84f,0.12f,theme.Orange,15);Button(tp,"TutorialSkip","ATLA",0.30f,0.85f,0.4f,0.09f,theme.Navy,11);
        Label(safe,"Toast","TESLİM EDİLDİ!",0.06f,0.46f,0.88f,0.06f,18,theme.Mint,TMPro.TextAlignmentOptions.Center).gameObject.AddComponent<RunFeedbackView>();
        var delivered=Object.Instantiate(target,street);delivered.name="DeliveredTarget";delivered.gameObject.SetActive(false);Field(cv,"deliveredTarget",delivered);Field(cv,"deliveredLabel",delivered.GetComponentInChildren<TMPro.TMP_Text>(true));
        refs["Timer"].gameObject.AddComponent<Canvas>();street.gameObject.AddComponent<Canvas>();refs["Toast"].gameObject.AddComponent<Canvas>();
        modal.gameObject.SetActive(false);game.gameObject.SetActive(false);cour.gameObject.SetActive(false);
        var controller=root.AddComponent<PaketciUIController>();Field(controller,"theme",theme);Field(controller,"config",AssetDatabase.LoadAssetAtPath<PaketciGameConfig>("Assets/GameData/PaketciGameConfig.asset"));Field(controller,"sorting",sorting);Field(controller,"courier",cv);
        var tutorial=tp.gameObject.AddComponent<TutorialController>();var choices=new List<Object>();for(int i=0;i<3;i++)choices.Add(Button(tp,"TutorialChoice"+i,i==0?"SOL":i==1?"ORTA":"SAĞ",0.065f+i*0.30f,0.61f,0.27f,0.11f,theme.Navy,11).GetComponent<UnityEngine.UI.Button>());Array(tutorial,"choices",choices);Field(tutorial,"next",refs["TutorialNext"].GetComponent<UnityEngine.UI.Button>());Field(tutorial,"body",refs["TutorialBody"].GetComponent<TMPro.TMP_Text>());Field(controller,"tutorial",tutorial);
        refs["TutorialBody"].anchorMin=new Vector2(0.08f,0.43f);refs["TutorialBody"].anchorMax=new Vector2(0.92f,0.59f);refs["TutorialNext"].anchorMin=new Vector2(0.08f,0.12f);refs["TutorialNext"].anchorMax=new Vector2(0.92f,0.24f);refs["TutorialSkip"].anchorMin=new Vector2(0.30f,0.01f);refs["TutorialSkip"].anchorMax=new Vector2(0.70f,0.10f);
        var serialized=new SerializedObject(controller);var elements=serialized.FindProperty("elements");elements.arraySize=refs.Count;int n=0;foreach(var pair in refs){var item=elements.GetArrayElementAtIndex(n++);item.FindPropertyRelative("Key").stringValue=pair.Key;item.FindPropertyRelative("Rect").objectReferenceValue=pair.Value;}serialized.ApplyModifiedPropertiesWithoutUndo();
        Directory.CreateDirectory("Assets/UI/Prefabs");PrefabUtility.SaveAsPrefabAsset(root,"Assets/UI/Prefabs/PaketciUI.prefab");return root;
    }
}
