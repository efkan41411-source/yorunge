using System;
using UnityEngine;

public interface IPaketciStore
{
    int GetInt(string key, int fallback);
    float GetFloat(string key, float fallback);
    string GetString(string key, string fallback);
    void SetInt(string key, int value);
    void SetFloat(string key, float value);
    void SetString(string key, string value);
    void Save();
}

public sealed class PlayerPrefsPaketciStore : IPaketciStore
{
    public int GetInt(string key, int fallback) => PlayerPrefs.GetInt(key, fallback);
    public float GetFloat(string key, float fallback) => PlayerPrefs.GetFloat(key, fallback);
    public string GetString(string key, string fallback) => PlayerPrefs.GetString(key, fallback);
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    public void Save() => PlayerPrefs.Save();
}

public sealed class PaketciSaveRepository
{
    private readonly IPaketciStore store;
    private readonly Func<DateTime> utcNow;
    private string rewardDate;
    public int Coins { get; private set; }
    public int Stars { get; private set; }
    public int UnlockedLevel { get; private set; }
    public int DailyMask { get; private set; }
    public bool Sound { get; private set; }
    public bool Haptics { get; private set; }
    public float Volume { get; private set; }
    public GameplayStyle PreferredStyle { get; private set; }
    public DateTime UtcNow => utcNow();
    public PaketciSaveRepository(IPaketciStore store, Func<DateTime> utcNow = null)
    {
        this.store = store; this.utcNow = utcNow ?? (() => DateTime.UtcNow);
        Coins = Mathf.Max(0, store.GetInt("Paketci.Coins", 0));
        Stars = Mathf.Max(0, store.GetInt("Paketci.Stars", 0));
        UnlockedLevel = Mathf.Max(1, store.GetInt("Paketci.UnlockedLevel", 1));
        Sound = store.GetInt("Paketci.Sound", 1) == 1;
        Haptics = store.GetInt("Paketci.Haptics", 1) == 1;
        Volume = Mathf.Clamp01(store.GetFloat("Paketci.Volume", 0.65f));
        PreferredStyle = (GameplayStyle)Mathf.Clamp(store.GetInt("Paketci.GameplayStyle", 0), 0, 1);
        RefreshDaily();
    }
    public void RefreshDaily()
    {
        string today = UtcNow.ToString("yyyy-MM-dd");
        if (rewardDate == today) return;
        rewardDate = today;
        if (store.GetString("Paketci.DailyDate", "") == today) DailyMask = store.GetInt("Paketci.DailyCompletedMask", 0) & 7;
        else { DailyMask = 0; store.SetString("Paketci.DailyDate", today); store.SetInt("Paketci.DailyCompletedMask", 0); Flush(); }
    }
    public bool DailyComplete(int contract) => (DailyMask & (1 << contract)) != 0;
    public int LevelStars(int level) => Mathf.Clamp(store.GetInt("Paketci.LevelStars." + level, 0), 0, 3);
    private static string BestKey(GameplayStyle style) => style == GameplayStyle.Courier ? "Paketci.RushBest" : "Paketci.SortingRushBest";
    public int Best(GameplayStyle style) => store.GetInt(BestKey(style), 0);
    public void RecordBest(GameplayStyle style, int score) => store.SetInt(BestKey(style), Mathf.Max(Best(style), score));
    public void AddCoins(int amount) { Coins = Mathf.Max(0, Coins + amount); store.SetInt("Paketci.Coins", Coins); }
    public int Complete(GameMode mode, int level, int contract, int stars)
    {
        if (mode == GameMode.Daily)
        {
            RefreshDaily();
            if (DailyComplete(contract)) return 0;
            DailyMask |= 1 << contract; store.SetInt("Paketci.DailyCompletedMask", DailyMask);
            Stars += stars; AddCoins(10 + contract * 5);
        }
        else if (mode == GameMode.Story)
        {
            int best = LevelStars(level); Stars += Mathf.Max(0, stars - best);
            store.SetInt("Paketci.LevelStars." + level, Mathf.Max(best, stars));
            UnlockedLevel = Mathf.Max(UnlockedLevel, level + 1); store.SetInt("Paketci.UnlockedLevel", UnlockedLevel);
            AddCoins(stars * 2);
        }
        store.SetInt("Paketci.Stars", Stars); Flush(); return stars;
    }
    public void SelectStyle(GameplayStyle style) { PreferredStyle = style; store.SetInt("Paketci.GameplayStyle", (int)style); Flush(); }
    public void Settings(bool sound, bool haptics, float volume)
    {
        Sound = sound; Haptics = haptics; Volume = Mathf.Clamp01(volume);
        store.SetInt("Paketci.Sound", Sound ? 1 : 0); store.SetInt("Paketci.Haptics", Haptics ? 1 : 0); store.SetFloat("Paketci.Volume", Volume); Flush();
    }
    public bool TutorialSeen(GameplayStyle style) => store.GetInt("Paketci.Tutorial.v2." + style, 0) == 1;
    public void MarkTutorial(GameplayStyle style) { store.SetInt("Paketci.Tutorial.v2." + style, 1); Flush(); }
    public void Flush() => store.Save();
}
