using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PaketciRuleChecks
{
    private sealed class MemoryStore : IPaketciStore
    {
        private readonly Dictionary<string,object> data=new Dictionary<string,object>();
        public int GetInt(string k,int f)=>data.TryGetValue(k,out var v)?(int)v:f;
        public float GetFloat(string k,float f)=>data.TryGetValue(k,out var v)?(float)v:f;
        public string GetString(string k,string f)=>data.TryGetValue(k,out var v)?(string)v:f;
        public void SetInt(string k,int v)=>data[k]=v;public void SetFloat(string k,float v)=>data[k]=v;public void SetString(string k,string v)=>data[k]=v;public void Save(){}
    }
    private static int checks;
    private static void Check(bool condition,string name){checks++;if(!condition)throw new Exception("FAILED: "+name);}
    private static void Solve(PaketciSession session)
    {
        while(session.State==GameState.Playing&&session.Sorting.CompletedCount<3)
        {
            var order=session.Sorting.Orders[session.Sorting.CompletedCount];
            foreach(int color in order.Recipe){int chosen=-1;for(int i=0;i<session.Sorting.Packages.Count;i++)if(session.Sorting.Available(i)&&session.Sorting.Packages[i].Color==color){chosen=i;break;}Check(chosen>=0,"Supply has recipe");session.Pick(chosen);}
            session.Tick(0.31f);
        }
    }
    [MenuItem("Tools/Paketci/Run Rule Checks")]
    public static string Run()
    {
        checks=0;var config=ScriptableObject.CreateInstance<PaketciGameConfig>();
        try
        {
            Check(SortingRules.CanMatch(new[]{2,0,1},new[]{0,1,2}),"Unordered recipe");Check(SortingRules.CanMatch(new[]{0,-1,0},new[]{0,0,2}),"Joker with repeated color");Check(!SortingRules.CanMatch(new[]{0,0,0},new[]{0,1,2}),"Wrong triple");Check(!SortingRules.CanMatch(new[]{7,1,2},new[]{0,1,2}),"Invalid color guarded");
            for(int seed=1;seed<=20;seed++){var a=new SortingRules();var b=new SortingRules();a.Build(seed,6,true,2);b.Build(seed,6,true,2);Check(a.Packages.Count==12,"Max twelve packages");for(int i=0;i<12;i++)Check(a.Packages[i].Color==b.Packages[i].Color,"Seed deterministic");}
            var c=new CourierRules();c.Start(123);for(int i=0;i<100;i++){int prior=c.TargetLane;c.Next();Check(prior!=c.TargetLane,"No consecutive target lane");}
            c.SetLane(-10);Check(c.Lane==0,"Lane min");c.SetLane(99);Check(c.Lane==2,"Lane max");
            c.Start(1);c.SetLane(c.TargetLane);c.Tick(0.5f,0,5);Check(c.Evaluate(0.18f,0.42f,0.35f)==DeliveryResult.Early,"Early throw");c.Tick(0.7f,1,5);Check(c.Evaluate(0.18f,0.42f,0.35f)==DeliveryResult.Perfect,"Perfect center");c.Tick(0.2f,1,5);Check(c.Evaluate(0.18f,0.42f,0.35f)==DeliveryResult.Late,"Late throw");
            DateTime date=new DateTime(2026,11,2,12,0,0,DateTimeKind.Utc);var store=new MemoryStore();store.SetInt("Paketci.UnlockedLevel",20);store.SetInt("Paketci.Coins",30);var save=new PaketciSaveRepository(store,()=>date);
            foreach(GameplayStyle style in Enum.GetValues(typeof(GameplayStyle)))foreach(GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                var s=new PaketciSession(config,save);s.StartRun(style,mode,1,0,false);Check(s.State==GameState.Playing,"Start "+style+mode);float time=s.Remaining;s.Pause();s.Tick(1);Check(s.Remaining==time,"Pause freezes timer");s.Resume();Check(s.State==GameState.Playing,"Resume");s.StartRun(style,mode,1,0,false);Check(s.Deliveries==0,"Restart resets");s.Menu();Check(s.State==GameState.Menu,"Menu state");
            }
            var story=new PaketciSession(config,save);story.StartRun(GameplayStyle.Sorting,GameMode.Story,1,0,false);story.Tick(1000);Check(story.State==GameState.Playing,"Story sorting untimed");int coins=save.Coins;story.Hint();Check(save.Coins==coins&&story.HintUsed,"First hint free");story.Hint();Check(save.Coins==coins-5,"Second hint costs five");Solve(story);Check(story.State==GameState.Completed&&story.StarsAwarded==3,"Story puzzle complete");int awarded=save.Coins;story.FinishRun(true);Check(save.Coins==awarded,"Finish idempotent");Check(save.LevelStars(1)==3,"Legacy stars key");
            var daily=new PaketciSession(config,save);daily.StartRun(GameplayStyle.Sorting,GameMode.Daily,1,1,false);Solve(daily);Check(daily.State==GameState.Completed&&save.DailyComplete(1),"Daily completion mask");int dailyCoins=save.Coins;daily.StartRun(GameplayStyle.Sorting,GameMode.Daily,1,1,false);Solve(daily);Check(save.Coins==dailyCoins&&daily.StarsAwarded==0,"Daily no duplicate reward");date=date.AddDays(1);save.RefreshDaily();Check(!save.DailyComplete(1),"UTC day refresh");
            var rush=new PaketciSession(config,save);rush.StartRun(GameplayStyle.Sorting,GameMode.Rush,1,0,false);rush.Tick(100);Check(rush.State==GameState.Completed,"Rush time is completed run");int run=rush.RunId;Check(rush.Revive(run),"Valid revive");Check(!rush.Revive(run),"Revive once");rush.StartRun(GameplayStyle.Sorting,GameMode.Rush,1,0,false);Check(!rush.Revive(run),"Stale callback guarded");
            var tut=new PaketciSession(config,save);tut.StartRun(GameplayStyle.Courier,GameMode.Story,1,0);Check(tut.State==GameState.Tutorial,"First tutorial");float before=tut.Remaining;tut.Tick(20);Check(tut.Remaining==before,"Tutorial freezes timer");tut.FinishTutorial();Check(tut.State==GameState.Playing&&save.TutorialSeen(GameplayStyle.Courier),"Tutorial persisted");
            return "PASS: "+checks+" assertions. In-memory store only; player progress unchanged.";
        }
        finally { UnityEngine.Object.DestroyImmediate(config); }
    }
}
