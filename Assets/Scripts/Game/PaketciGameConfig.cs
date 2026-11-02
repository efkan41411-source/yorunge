using UnityEngine;

public enum GameMode { Story, Rush, Daily }
public enum GameplayStyle { Courier, Sorting }
public enum GameState { Menu, Playing, Paused, Tutorial, Completed, Failed }

[CreateAssetMenu(menuName = "Paketci/Game Config")]
public sealed class PaketciGameConfig : ScriptableObject
{
    [SerializeField] private float rushDuration = 75f;
    [SerializeField] private float storyDuration = 65f;
    [SerializeField] private float reviveDuration = 30f;
    [SerializeField] private float windowStart = 0.18f;
    [SerializeField] private float windowEnd = 0.42f;
    [SerializeField, Range(0.1f, 0.8f)] private float perfectWindowFraction = 0.35f;
    [SerializeField] private float laneSpeed = 5f;
    public float RushDuration => rushDuration;
    public float ReviveDuration => reviveDuration;
    public float LaneSpeed => laneSpeed;
    public float PerfectWindowFraction => perfectWindowFraction;
    public float Duration(GameMode mode, int level, int contract) => mode == GameMode.Rush ? rushDuration : mode == GameMode.Daily ? DailyDuration(contract) : storyDuration + Mathf.Min(20, Mathf.Max(0, level - 1) * 2);
    public float WindowStart(GameMode mode, int contract) => mode == GameMode.Daily && contract == 2 ? 0.23f : windowStart;
    public float WindowEnd(GameMode mode, int contract) => mode == GameMode.Daily && contract == 2 ? 0.35f : windowEnd;
    public float ApproachSpeed(GameMode mode, int level, int contract, int score) => mode == GameMode.Rush ? 0.32f + Mathf.Min(score, 24) * 0.009f : mode == GameMode.Daily ? 0.32f + contract * 0.045f : 0.32f + Mathf.Min(level - 1, 15) * 0.008f;
    public static int DailyDuration(int contract) => contract == 0 ? 90 : contract == 1 ? 75 : 60;
    public static int DeliveryGoal(int level) => 3 + Mathf.Min(6, Mathf.Max(0, level - 1) / 2);
    public static int ColorCount(int level) => level >= 13 ? 6 : level >= 8 ? 5 : level >= 4 ? 4 : 3;
    public static string District(int level) => level >= 13 ? "Liman Mahallesi" : level >= 8 ? "Eski Çarşı" : level >= 4 ? "Çiçekli Sokak" : "İstasyon Caddesi";
    public static string Scenario(int contract) => contract == 0 ? "Sahil Rotası" : contract == 1 ? "Pazar Yoğunluğu" : "Akşam Trafiği";
}
