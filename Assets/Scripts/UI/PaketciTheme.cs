using UnityEngine;

[CreateAssetMenu(menuName = "Paketci/UI Theme")]
public sealed class PaketciTheme : ScriptableObject
{
    [SerializeField] private Sprite panel;
    [SerializeField] private Sprite[] icons;
    [SerializeField] private Sprite town;
    [SerializeField] private Sprite courier;
    [SerializeField] private Sprite mascot;
    [SerializeField] private TMPro.TMP_FontAsset font;
    public Sprite Panel => panel;
    public Sprite Icon(int index) => icons[Mathf.Clamp(index, 0, icons.Length - 1)];
    public Sprite Town => town;
    public Sprite Courier => courier;
    public Sprite Mascot => mascot;
    public TMPro.TMP_FontAsset Font => font;
    public Color Navy => new Color(0.075f, 0.12f, 0.19f);
    public Color Surface => new Color(0.14f, 0.21f, 0.30f);
    public Color Cream => new Color(1f, 0.96f, 0.88f);
    public Color Orange => new Color(1f, 0.59f, 0.25f);
    public Color Mint => new Color(0.39f, 0.85f, 0.67f);
    public Color Muted => new Color(0.65f, 0.73f, 0.80f);
    public Color PackageColor(int color)
    {
        switch (color) { case 0: return new Color(0.91f,0.34f,0.36f); case 1: return new Color(0.96f,0.74f,0.28f); case 2: return new Color(0.28f,0.63f,0.9f); case 3: return new Color(0.31f,0.73f,0.55f); case 4: return new Color(0.68f,0.48f,0.85f); case 5: return Orange; default: return Cream; }
    }
    public static string ColorName(int color) { string[] names = { "Kırmızı", "Sarı", "Mavi", "Yeşil", "Mor", "Turuncu" }; return color < 0 ? "Joker" : names[color]; }
}
