using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

public static class PaketciRuntimeChecks
{
    private static PaketciUIController ui;
    private static Dictionary<string,RectTransform> refs;
    private static int stage, frame, savedStyle;
    private static float nextAt;
    private static readonly List<string> shots=new List<string>();
    private static UnityEngine.UI.Button Button(string name)=>refs[name].GetComponent<UnityEngine.UI.Button>();
    private static void Check(bool value,string name){Debug.Log("[Test] "+name+" = "+value);if(!value)throw new Exception(name);}
    public static void Setup()
    {
        var root=SceneManager.GetActiveScene().GetRootGameObjects().Single(g=>g.name=="PaketciUI");ui=root.GetComponent<PaketciUIController>();refs=root.GetComponentsInChildren<RectTransform>(true).GroupBy(r=>r.name).ToDictionary(g=>g.Key,g=>g.First());savedStyle=PlayerPrefs.GetInt("Paketci.GameplayStyle",0);
        Check(ui.Session!=null,"Session initialized");Check(ui.Session.State==GameState.Menu,"Menu initial");Check(SceneManager.GetActiveScene().GetRootGameObjects().Count(g=>g.GetComponent<PaketciUIController>()!=null)==1,"Single controller");Check(UnityEngine.EventSystems.EventSystem.current!=null,"EventSystem exists");
        SetGameViewSize(390,780);stage=frame=0;nextAt=Time.unscaledTime+0.6f;shots.Clear();
    }
    private static void Screenshot(string name)
    {
        string path="Assets/Tests/"+name+".png";ScreenCapture.CaptureScreenshot(path);shots.Add(path);SessionState.SetString("PlayModeTest.ScreenshotPath",path);Debug.Log("[Test] Screenshot "+path+" "+Screen.width+"x"+Screen.height);
    }
    public static bool Tick()
    {
        frame++;if(Time.unscaledTime<nextAt)return false;
        nextAt=Time.unscaledTime+0.3f;
        if(stage==0)Screenshot("MenuPreview");
        else if(stage==1){ui.Session.StartRun(GameplayStyle.Sorting,GameMode.Story,1,0,false);ui.Session.Sorting.Build(123,6,true,2);ui.Refresh();}
        else if(stage==2)Screenshot("SortingPreview");
        else if(stage==3){Check(refs["Package11"].gameObject.activeSelf,"Twelve package cards");Check(refs["PackageContent"].rect.height>refs["Viewport"].rect.height,"Scrollable package supply");Button("Package0").onClick.Invoke();Check(ui.Session.Sorting.Tray.Count==1,"Package onClick selects once");Button("Undo").onClick.Invoke();Check(ui.Session.Sorting.Tray.Count==0,"Undo works");var point=RectTransformUtility.WorldToScreenPoint(null,refs["Pause"].position);var data=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=point};var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(data,hits);Check(hits.Any(h=>h.gameObject.GetComponentInParent<UnityEngine.UI.Button>()==Button("Pause")),"Pause receives raycast");Button("Pause").onClick.Invoke();Check(ui.Session.State==GameState.Paused,"Pause wired");}
        else if(stage==4)Screenshot("PausePreview");
        else if(stage==5)Button("PauseSettings").onClick.Invoke();
        else if(stage==6)Screenshot("SettingsPreview");
        else if(stage==7){Button("CloseSettings").onClick.Invoke();Check(ui.Session.State==GameState.Paused,"Settings returns to pause");Button("Resume").onClick.Invoke();Check(ui.Session.State==GameState.Playing,"Resume wired");ui.Session.StartRun(GameplayStyle.Courier,GameMode.Story,1,0,false);Button("Lane"+ui.Session.Courier.TargetLane).onClick.Invoke();}
        else if(stage==8){Check(Mathf.Abs(ui.Session.Courier.LanePosition-ui.Session.Courier.TargetLane)<0.09f,"Visual courier reaches target lane");Screenshot("CourierPreview");}
        else if(stage==9)ui.Session.FinishRun(false);
        else if(stage==10){Check(refs["ResultPanel"].gameObject.activeInHierarchy,"Result modal visible");Screenshot("ResultPreview");}
        else if(stage==11){Button("ResultMenu").onClick.Invoke();Check(ui.Session.State==GameState.Menu,"Result menu wired");SetGameViewSize(390,694);}
        else if(stage==12)Screenshot("SmallPhonePreview");
        else if(stage==13)SetGameViewSize(390,867);
        else if(stage==14)Screenshot("TallPhonePreview");
        else if(stage==15)SetGameViewSize(768,1024);
        else if(stage==16)Screenshot("TabletPreview");
        else if(stage==17)SetGameViewSize(390,780);
        else if(stage==18)return true;
        stage++;return false;
    }
    public static string Finish()
    {
        PlayerPrefs.SetInt("Paketci.GameplayStyle",savedStyle);PlayerPrefs.Save();
        return "Runtime UI flows passed; screenshots: "+string.Join(", ",shots);
    }
    public static void SetGameViewSize(int w,int h)
    {
        var assembly=typeof(Editor).Assembly;var sizesType=assembly.GetType("UnityEditor.GameViewSizes");var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");var groupType=assembly.GetType("UnityEditor.GameViewSizeGroupType");var instance=sizesType.BaseType.GetProperty("instance",BindingFlags.Public|BindingFlags.Static).GetValue(null,null);var group=sizesType.GetMethod("GetGroup").Invoke(instance,new[]{Enum.Parse(groupType,"Android")});int builtin=(int)group.GetType().GetMethod("GetBuiltinCount").Invoke(group,null);int custom=(int)group.GetType().GetMethod("GetCustomCount").Invoke(group,null);var constructor=sizeType.GetConstructor(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new[]{kind,typeof(int),typeof(int),typeof(string)},null);var size=constructor.Invoke(new[]{Enum.Parse(kind,"FixedResolution"),(object)w,h,"Paketci "+w+"x"+h});group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});var viewType=assembly.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,builtin+custom,null);view.Focus();
    }
}
