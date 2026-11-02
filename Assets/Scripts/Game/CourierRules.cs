using UnityEngine;

public enum DeliveryResult { Moving, WrongLane, Early, Late, Good, Perfect }
public sealed class CourierRules
{
    private System.Random random;
    public int Lane { get; private set; } = 1;
    public float LanePosition { get; private set; } = 1f;
    public int TargetLane { get; private set; }
    public float Distance { get; private set; } = 1f;
    public float Scroll { get; private set; }
    public void Start(int seed) { random = new System.Random(seed); Lane = 1; LanePosition = 1f; TargetLane = -1; Scroll = 0; Next(); }
    public void SetLane(int lane) { Lane = Mathf.Clamp(lane, 0, 2); }
    public void Tick(float delta, float speed, float laneSpeed)
    {
        Distance -= speed * delta; Scroll = (Scroll + delta * (0.25f + speed)) % 1f;
        LanePosition = Mathf.MoveTowards(LanePosition, Lane, delta * laneSpeed);
    }
    public void Next()
    {
        int next = random.Next(3);
        if (next == TargetLane) next = (next + 1 + random.Next(2)) % 3;
        TargetLane = next; Distance = 1f;
    }
    public DeliveryResult Evaluate(float start, float end, float perfectFraction)
    {
        if (Lane != TargetLane) return DeliveryResult.WrongLane;
        if (Mathf.Abs(LanePosition - Lane) > 0.08f) return DeliveryResult.Moving;
        if (Distance > end) return DeliveryResult.Early;
        if (Distance < start) return DeliveryResult.Late;
        float center = (start + end) * 0.5f;
        return Mathf.Abs(Distance - center) <= (end - start) * perfectFraction * 0.5f ? DeliveryResult.Perfect : DeliveryResult.Good;
    }
}
