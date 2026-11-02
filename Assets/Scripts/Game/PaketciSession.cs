using System;
using UnityEngine;

public sealed class PaketciSession
{
    private readonly PaketciGameConfig config;
    private float selectionLock;
    private int wave;
    public PaketciSaveRepository Save { get; }
    public SortingRules Sorting { get; } = new SortingRules();
    public CourierRules Courier { get; } = new CourierRules();
    public GameState State { get; private set; } = GameState.Menu;
    public GameplayStyle Style { get; private set; }
    public GameMode Mode { get; private set; }
    public int Level { get; private set; }
    public int Contract { get; private set; }
    public int RunId { get; private set; }
    public int Deliveries { get; private set; }
    public int Mistakes { get; private set; }
    public int Combo { get; private set; }
    public int StarsAwarded { get; private set; }
    public int EarnedCoins { get; private set; }
    public int PerfectCount { get; private set; }
    public float Remaining { get; private set; }
    public float Duration => config.Duration(Mode, Level, Contract);
    public float WindowStart => config.WindowStart(Mode, Contract);
    public float WindowEnd => config.WindowEnd(Mode, Contract);
    public float PerfectFraction => config.PerfectWindowFraction;
    public int Goal => Style == GameplayStyle.Sorting ? 3 : Mode == GameMode.Daily ? 4 + Contract * 2 : PaketciGameConfig.DeliveryGoal(Level);
    public bool Timed => Style == GameplayStyle.Courier || Mode != GameMode.Story;
    public bool ReviveUsed { get; private set; }
    public bool HintUsed { get; private set; }
    public int HintIndex { get; private set; } = -1;
    public float HintUntil { get; private set; }
    public bool CanPick(int index) => State == GameState.Playing && Style == GameplayStyle.Sorting && selectionLock <= 0 && Sorting.Tray.Count < 3 && Sorting.Available(index);
    public string Message { get; private set; } = "Mahallene hoş geldin!";
    public event Action Changed;
    public event Action<string, bool> Feedback;
    public event Action<float, int, float> Thrown;
    public PaketciSession(PaketciGameConfig config, PaketciSaveRepository save) { this.config = config; Save = save; Style = save.PreferredStyle; }
    public void StartRun(GameplayStyle style, GameMode mode, int level, int contract, bool tutorial = true)
    {
        RunId++; Style = style; Mode = mode; Level = Mathf.Clamp(level, 1, Save.UnlockedLevel); Contract = Mathf.Clamp(contract, 0, 2);
        Deliveries = Mistakes = Combo = StarsAwarded = EarnedCoins = PerfectCount = wave = 0;
        ReviveUsed = HintUsed = false; HintIndex = -1; selectionLock = 0;
        Save.RefreshDaily(); Save.SelectStyle(style); Remaining = Timed ? Duration : 0;
        int seed = mode == GameMode.Daily ? DailySeed(Save.UtcNow, Contract) : mode == GameMode.Story ? Level * 7919 : Environment.TickCount;
        if (style == GameplayStyle.Courier) Courier.Start(seed); else BuildWave(seed);
        Message = style == GameplayStyle.Courier ? "Hedef şeridine geç, yeşil alanda fırlat." : "Üç uygun paketi seç, siparişi tamamla.";
        State = tutorial && !Save.TutorialSeen(style) ? GameState.Tutorial : GameState.Playing; Notify();
    }
    private void BuildWave(int seed)
    {
        int difficulty = Mode == GameMode.Rush ? 1 + Deliveries / 3 : Level;
        int colors = Mode == GameMode.Daily ? 3 + Contract : PaketciGameConfig.ColorCount(difficulty);
        int decoys = Mode == GameMode.Daily ? Contract : Mode == GameMode.Rush && wave > 0 ? Mathf.Min(2, 1 + Deliveries / 5) : difficulty >= 7 ? 2 : difficulty >= 3 ? 1 : 0;
        Sorting.Build(seed, colors, Mode != GameMode.Daily && difficulty >= 3, decoys); wave++;
    }
    public void Tick(float delta)
    {
        if (State != GameState.Playing) return;
        selectionLock = Mathf.Max(0, selectionLock - delta);
        if (!Timed) return;
        Remaining = Mathf.Max(0, Remaining - delta);
        if (Style == GameplayStyle.Courier)
        {
            Courier.Tick(delta, config.ApproachSpeed(Mode, Level, Contract, Deliveries), config.LaneSpeed);
            if (Courier.Distance <= 0 && Remaining > 0) Miss("Hedefi kaçırdın!");
        }
        if (Remaining <= 0) FinishRun(false);
    }
    public void FinishTutorial() { if (State != GameState.Tutorial) return; Save.MarkTutorial(Style); State = GameState.Playing; Notify(); }
    public void Pause() { if (State == GameState.Playing) { State = GameState.Paused; Notify(); } }
    public void Resume() { if (State == GameState.Paused) { State = GameState.Playing; Notify(); } }
    public void Menu() { RunId++; State = GameState.Menu; Save.RefreshDaily(); Save.Flush(); Notify(); }
    public void SetLane(int lane) { if (State != GameState.Playing || Style != GameplayStyle.Courier) return; Courier.SetLane(lane); Notify(); }
    public void Throw()
    {
        if (State != GameState.Playing || Style != GameplayStyle.Courier) return;
        var result = Courier.Evaluate(WindowStart, WindowEnd, PerfectFraction);
        if (result == DeliveryResult.Moving) { Message = "Şerit değiştiriliyor…"; Notify(); return; }
        if (result == DeliveryResult.WrongLane) { Miss("Yanlış şerit!"); return; }
        if (result == DeliveryResult.Early) { Miss("Çok erken fırlattın!"); return; }
        if (result == DeliveryResult.Late) { Miss("Geç kaldın!"); return; }
        Thrown?.Invoke(Courier.LanePosition, Courier.TargetLane, Courier.Distance);
        if (result == DeliveryResult.Perfect) PerfectCount++;
        Deliveries++; Combo++;
        Reward(Mode == GameMode.Daily ? 0 : 5 + Mathf.Min(Combo - 1, 4));
        if (Mode == GameMode.Rush) { Remaining = Mathf.Min(Duration, Remaining + 2.5f); Save.RecordBest(Style, Deliveries); }
        Message = result == DeliveryResult.Perfect ? "MÜKEMMEL!  Seri x" + Combo : "TESLİM EDİLDİ!  Seri x" + Combo;
        Feedback?.Invoke(Message, true); Courier.Next();
        if (Mode != GameMode.Rush && Deliveries >= Goal) FinishRun(true);
        Save.Flush(); Notify();
    }
    public void Pick(int index)
    {
        if (State != GameState.Playing || Style != GameplayStyle.Sorting || selectionLock > 0) return;
        int result = Sorting.Pick(index); if (result == -2) return;
        HintIndex = -1;
        if (result == -1) Message = "Tepsi: " + Sorting.Tray.Count + "/3 • Bir paket daha seç.";
        else if (result == 0)
        {
            Mistakes++; Combo = 0; Message = "Bu üçlü eşleşmedi. Geri alıp yeniden dene.";
            Feedback?.Invoke(Message, false); if (Mode == GameMode.Rush) Remaining = Mathf.Max(0, Remaining - 5);
            if (Timed && Remaining <= 0) FinishRun(false);
        }
        else
        {
            Deliveries++; Combo++; selectionLock = 0.3f;
            Reward(Mode == GameMode.Daily ? 0 : 5 + Mathf.Min(Combo - 1, 3));
            Message = "DOĞRU PAKETLER!  Seri x" + Combo; Feedback?.Invoke(Message, true);
            if (Mode == GameMode.Rush) { Remaining = Mathf.Min(Duration, Remaining + 8); Save.RecordBest(Style, Deliveries); }
            if (Sorting.CompletedCount == 3)
            {
                if (Mode == GameMode.Rush) { BuildWave(Environment.TickCount ^ (Deliveries * 7919)); HintUsed = false; }
                else FinishRun(true);
            }
            Save.Flush();
        }
        Notify();
    }
    public void Undo() { if (State != GameState.Playing || Style != GameplayStyle.Sorting) return; Sorting.Undo(); Message = "Son paket geri alındı."; Notify(); }
    public void Hint()
    {
        if (State != GameState.Playing || Style != GameplayStyle.Sorting) return;
        if (HintUsed && Save.Coins < 5) { Message = "İpucu için 5 jeton gerekiyor."; Notify(); return; }
        int index = Sorting.Hint();
        if (index < 0) { Message = "Önce tepsiden bir paketi geri al."; Notify(); return; }
        if (HintUsed) { Save.AddCoins(-5); Save.Flush(); }
        HintUsed = true; HintIndex = index; HintUntil = Time.unscaledTime + 3; Message = "Parlayan paket açık siparişe uygun."; Notify();
    }
    private void Reward(int amount) { Save.AddCoins(amount); EarnedCoins += amount; }
    private void Miss(string message)
    {
        if (State != GameState.Playing) return;
        Mistakes++; Combo = 0; Remaining = Mathf.Max(0, Remaining - 4); Message = message + "  −4 sn";
        Feedback?.Invoke(Message, false); Courier.Next(); if (Remaining <= 0) FinishRun(false); Notify();
    }
    public void FinishRun(bool success)
    {
        if (State != GameState.Playing) return;
        State = success || Mode == GameMode.Rush ? GameState.Completed : GameState.Failed;
        if (Mode == GameMode.Rush) Save.RecordBest(Style, Deliveries);
        else if (success)
        {
            int before = Save.Coins;
            StarsAwarded = Save.Complete(Mode, Level, Contract, Mistakes == 0 ? 3 : Mistakes <= 2 ? 2 : 1);
            EarnedCoins += Save.Coins - before;
        }
        Save.Flush(); Notify();
    }
    public bool Revive(int runId)
    {
        if (runId != RunId || ReviveUsed || !(State == GameState.Failed || (State == GameState.Completed && Mode == GameMode.Rush))) return false;
        ReviveUsed = true; Remaining = config.ReviveDuration; State = GameState.Playing;
        if (Style == GameplayStyle.Courier) Courier.Next(); Message = "+30 saniye! Devam et."; Notify(); return true;
    }
    public static int DailySeed(DateTime date, int contract) => unchecked((date.Year * 10000 + date.Month * 100 + date.Day) * 31 + contract * 7919);
    private void Notify() => Changed?.Invoke();
}
