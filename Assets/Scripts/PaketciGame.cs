using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1000)]
public sealed class PaketciGame : MonoBehaviour
{
    private const float DesignWidth = 390f;
    private const float DesignHeight = 780f;
    private const float RushDuration = 75f;
    private const float DailyDuration = 120f;

    private enum GameMode
    {
        Story,
        Rush,
        Daily
    }

    private static readonly Color[] PackageColors =
    {
        new Color(0.91f, 0.32f, 0.30f),
        new Color(0.96f, 0.72f, 0.22f),
        new Color(0.28f, 0.62f, 0.87f),
        new Color(0.30f, 0.67f, 0.47f),
        new Color(0.62f, 0.44f, 0.77f),
        new Color(0.94f, 0.49f, 0.28f)
    };

    private static readonly string[] PackageColorNames = { "KIRMIZI", "SARI", "MAVİ", "YEŞİL", "MOR", "TURUNCU" };
    private static readonly Color[] TextColors =
    {
        new Color(0.13f, 0.18f, 0.27f),
        new Color(0.40f, 0.45f, 0.54f),
        new Color(0.98f, 0.97f, 0.91f),
        new Color(0.22f, 0.55f, 0.39f),
        new Color(0.75f, 0.27f, 0.24f),
        new Color(0.96f, 0.75f, 0.31f),
        new Color(0.91f, 0.90f, 0.83f)
    };

    private readonly List<Package> packages = new List<Package>();
    private readonly List<Order> orders = new List<Order>();
    private readonly List<int> tray = new List<int>(3);
    private readonly Dictionary<int, GUIStyle> textStyles = new Dictionary<int, GUIStyle>();
    private readonly List<Texture2D> generatedTextures = new List<Texture2D>();
    private readonly List<AudioClip> generatedAudioClips = new List<AudioClip>();

    private Texture2D circleTexture;
    private AudioSource uiAudioSource;
    private AudioClip tapClip;
    private AudioClip successClip;
    private AudioClip errorClip;
    private GUIStyle primaryButtonStyle;
    private GUIStyle secondaryButtonStyle;
    private GUIStyle hintButtonStyle;
    private GUIStyle transparentButtonStyle;
    private bool stylesReady;
    private bool onMenu = true;
    private bool levelComplete;
    private bool runFailed;
    private bool paused;
    private bool settingsOpen;
    private bool levelSelectOpen;
    private bool soundEnabled = true;
    private bool hapticsEnabled = true;
    private bool hintUsed;
    private bool dailyRewardGranted;
    private GameMode selectedMode;
    private float masterVolume = 0.65f;
    private int level = 1;
    private int selectedLevel = 1;
    private int levelPage;
    private int unlockedLevel = 1;
    private int coins;
    private int totalStars;
    private int mistakes;
    private int starsAwarded;
    private int deliveryStreak;
    private int lastDeliveryReward;
    private int modeBestScore;
    private int rushScore;
    private int highlightedPackage = -1;
    private float remainingTime;
    private float hintExpiresAt;
    private float deliveryCelebrationUntil;
    private float wrongAnswerUntil;
    private string message = "Üç paketi tepsiye koyup bir siparişi tamamla.";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<PaketciGame>() != null)
        {
            return;
        }

        GameObject game = new GameObject("Paketci Game");
        game.AddComponent<PaketciGame>();
    }

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        unlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt("Paketci.UnlockedLevel", 1));
        selectedLevel = unlockedLevel;
        coins = Mathf.Max(0, PlayerPrefs.GetInt("Paketci.Coins", 0));
        totalStars = Mathf.Max(0, PlayerPrefs.GetInt("Paketci.Stars", 0));
        soundEnabled = PlayerPrefs.GetInt("Paketci.Sound", 1) == 1;
        hapticsEnabled = PlayerPrefs.GetInt("Paketci.Haptics", 1) == 1;
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("Paketci.Volume", 0.65f));
        modeBestScore = Mathf.Max(0, PlayerPrefs.GetInt("Paketci.RushBest", 0));
        RefreshDailyRewardState();
        CreateAudioFeedback();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < generatedTextures.Count; i++)
        {
            Destroy(generatedTextures[i]);
        }

        for (int i = 0; i < generatedAudioClips.Count; i++)
        {
            Destroy(generatedAudioClips[i]);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame))
        {
            if (settingsOpen)
            {
                settingsOpen = false;
            }
            else if (levelSelectOpen)
            {
                levelSelectOpen = false;
            }
            else if (!onMenu && !levelComplete && !runFailed)
            {
                paused = !paused;
            }
        }

        if (onMenu || paused || settingsOpen || levelComplete || runFailed || selectedMode == GameMode.Story)
        {
            return;
        }

        remainingTime -= Time.unscaledDeltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            FinishTimedRun();
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused && !onMenu && !levelComplete && !runFailed)
        {
            paused = true;
            PlayerPrefs.Save();
        }
    }

    private void FinishTimedRun()
    {
        runFailed = true;
        remainingTime = 0f;
        PlaySound(errorClip);
        Vibrate();
        if (selectedMode == GameMode.Rush)
        {
            modeBestScore = Mathf.Max(modeBestScore, rushScore);
            PlayerPrefs.SetInt("Paketci.RushBest", modeBestScore);
            PlayerPrefs.Save();
        }
    }

    private void OnGUI()
    {
        EnsureStyles();

        float scale = Mathf.Min(Screen.width / DesignWidth, Screen.height / DesignHeight);
        float offsetX = (Screen.width - DesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - DesignHeight * scale) * 0.5f;
        Matrix4x4 previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.12f, 0.16f, 0.25f));
        DrawBackdrop();

        if (onMenu)
        {
            DrawMenu();
        }
        else
        {
            DrawGame();
        }

        GUI.matrix = previousMatrix;
    }

    private void DrawMenu()
    {
        DrawTown();
        if (settingsOpen)
        {
            DrawSettingsOverlay(false);
            return;
        }
        if (levelSelectOpen)
        {
            DrawLevelSelectOverlay();
            return;
        }

        if (GUI.Button(new Rect(24f, 22f, 128f, 36f), "BÖLÜM HARİTASI", secondaryButtonStyle))
        {
            levelSelectOpen = true;
            PlaySound(tapClip);
        }
        if (GUI.Button(new Rect(283f, 22f, 83f, 36f), "AYARLAR", secondaryButtonStyle))
        {
            settingsOpen = true;
            PlaySound(tapClip);
        }

        DrawPanel(new Rect(25f, 72f, 340f, 204f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawRect(new Rect(43f, 89f, 304f, 28f), new Color(0.94f, 0.88f, 0.74f));
        DrawText(new Rect(46f, 87f, 298f, 30f), "MAHALLE KARGO  •  08:15", 13, TextAnchor.MiddleCenter, 1, true);
        DrawMascot(new Rect(155f, 127f, 80f, 74f));
        DrawText(new Rect(38f, 201f, 314f, 49f), "PAKETÇİ", 39, TextAnchor.MiddleCenter, 0, true);
        DrawText(new Rect(38f, 243f, 314f, 24f), "SON TESLİMAT", 15, TextAnchor.MiddleCenter, 3, true);

        DrawText(new Rect(25f, 286f, 340f, 26f), "BUGÜN NASIL OYNAYACAKSIN?", 13, TextAnchor.MiddleLeft, 5, true);
        DrawModeCard(new Rect(25f, 316f, 108f, 100f), "MAHALLE", "Sakin ve sınırsız", GameMode.Story);
        DrawModeCard(new Rect(141f, 316f, 108f, 100f), "EKSPRES", "75 saniyelik yarış", GameMode.Rush);
        DrawModeCard(new Rect(257f, 316f, 108f, 100f), "GÜNLÜK", "Herkese aynı rota", GameMode.Daily);

        DrawStatCard(new Rect(25f, 431f, 104f, 65f), "BÖLÜM", selectedLevel.ToString("00"), PackageColors[0]);
        DrawStatCard(new Rect(143f, 431f, 104f, 65f), "JETON", coins.ToString(), PackageColors[1]);
        DrawStatCard(new Rect(261f, 431f, 104f, 65f), "YILDIZ", totalStars.ToString(), PackageColors[4]);

        DrawPanel(new Rect(25f, 507f, 340f, 42f), new Color(0.20f, 0.26f, 0.36f), new Color(0.30f, 0.38f, 0.48f));
        DrawText(new Rect(36f, 512f, 318f, 32f), SelectedModeSummary(), 12, TextAnchor.MiddleCenter, 2, false);

        if (GUI.Button(new Rect(25f, 563f, 340f, 66f), "TESLİMATA BAŞLA    ›", primaryButtonStyle))
        {
            level = selectedMode == GameMode.Story ? selectedLevel : 1;
            StartLevel();
            onMenu = false;
            paused = false;
            levelSelectOpen = false;
            settingsOpen = false;
            PlaySound(tapClip);
        }

        DrawPanel(new Rect(25f, 640f, 340f, 55f), new Color(0.20f, 0.26f, 0.36f), new Color(0.30f, 0.38f, 0.48f));
        DrawText(new Rect(35f, 645f, 320f, 45f), "Her bölümde yeni bir sipariş bulmacası.\nYıldızlar ilerlemeni kaydeder.", 13, TextAnchor.MiddleCenter, 2, false);

    }

    private void DrawModeCard(Rect rect, string title, string subtitle, GameMode mode)
    {
        bool selected = selectedMode == mode;
        Color background = selected ? new Color(0.98f, 0.95f, 0.86f) : new Color(0.20f, 0.26f, 0.36f);
        Color outline = selected ? PackageColors[1] : new Color(0.30f, 0.38f, 0.48f);
        DrawPanel(rect, background, outline);
        if (GUI.Button(rect, GUIContent.none, transparentButtonStyle))
        {
            selectedMode = mode;
            PlaySound(tapClip);
        }

        string icon = mode == GameMode.Story ? "∞" : mode == GameMode.Rush ? "75" : "24H";
        DrawCircle(new Rect(rect.center.x - 18f, rect.y + 8f, 36f, 36f), selected ? PackageColors[1] : new Color(0.34f, 0.43f, 0.54f));
        DrawText(new Rect(rect.center.x - 18f, rect.y + 8f, 36f, 36f), icon, mode == GameMode.Daily ? 10 : 14,
            TextAnchor.MiddleCenter, selected ? 0 : 2, true);
        DrawText(new Rect(rect.x + 3f, rect.y + 47f, rect.width - 6f, 20f), title, 12, TextAnchor.MiddleCenter, selected ? 0 : 2, true);
        DrawText(new Rect(rect.x + 3f, rect.y + 68f, rect.width - 6f, 27f), subtitle, 9, TextAnchor.MiddleCenter, selected ? 1 : 6, false);
    }

    private string SelectedModeSummary()
    {
        if (selectedMode == GameMode.Rush)
        {
            return "75 sn • Teslimatlar süre kazandırır • Rekor: " + modeBestScore;
        }
        if (selectedMode == GameMode.Daily)
        {
            return dailyRewardGranted ? "Bugünün ödülü alındı • Yarın yeni rota" : "Her gün değişen bulmaca • İlk tamamlamaya ödül";
        }
        return "Hikâye modu • " + DistrictName(selectedLevel) + " • Joker paket bölüm 3'te açılır";
    }

    private string ModeShortName()
    {
        return selectedMode == GameMode.Story ? "HİKÂYE" : selectedMode == GameMode.Rush ? "EKSPRES" : "GÜNLÜK";
    }

    private string ModeLevelLabel()
    {
        if (selectedMode == GameMode.Story)
        {
            return "BÖLÜM  #" + level.ToString("00");
        }
        return selectedMode == GameMode.Rush ? "TESLİMAT YARIŞI" : "GÜNLÜK MEYDAN OKUMA";
    }

    private static string DistrictName(int levelNumber)
    {
        if (levelNumber >= 13)
        {
            return "LİMAN MAHALLESİ";
        }
        if (levelNumber >= 8)
        {
            return "ESKİ ÇARŞI";
        }
        if (levelNumber >= 4)
        {
            return "ÇİÇEKLİ SOKAK";
        }
        return "İSTASYON CADDESİ";
    }

    private static string CurrentDateKey()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd");
    }

    private void RefreshDailyRewardState()
    {
        dailyRewardGranted = PlayerPrefs.GetString("Paketci.DailyRewardDate", string.Empty) == CurrentDateKey();
    }

    private void ReturnToMenu()
    {
        onMenu = true;
        paused = false;
        settingsOpen = false;
        levelSelectOpen = false;
        levelComplete = false;
        runFailed = false;
        selectedLevel = Mathf.Clamp(selectedLevel, 1, unlockedLevel);
        RefreshDailyRewardState();
        PlayerPrefs.Save();
    }

    private void DrawGame()
    {
        if (settingsOpen)
        {
            DrawSettingsOverlay(true);
            return;
        }
        if (paused)
        {
            DrawPauseOverlay();
            return;
        }
        DrawPanel(new Rect(15f, 13f, 360f, 64f), new Color(0.20f, 0.26f, 0.36f), new Color(0.30f, 0.38f, 0.48f));
        DrawText(new Rect(28f, 20f, 188f, 22f), "PAKETÇİ  /  " + ModeShortName(), 12, TextAnchor.MiddleLeft, 5, true);
        DrawText(new Rect(28f, 44f, 188f, 22f), ModeLevelLabel(), 15, TextAnchor.MiddleLeft, 2, true);
        DrawPanel(new Rect(231f, 25f, 76f, 37f), new Color(0.30f, 0.35f, 0.42f), new Color(0.40f, 0.46f, 0.53f));
        DrawCircle(new Rect(239f, 34f, 18f, 18f), PackageColors[1]);
        DrawText(new Rect(260f, 28f, 42f, 29f), coins.ToString(), 15, TextAnchor.MiddleLeft, 2, true);
        if (!levelComplete && !runFailed && GUI.Button(new Rect(317f, 22f, 45f, 43f), "Ⅱ", secondaryButtonStyle))
        {
            paused = true;
            PlaySound(tapClip);
        }

        string orderHeader = selectedMode == GameMode.Daily ? "Günün siparişleri"
            : selectedMode == GameMode.Rush ? "Hızlı rota " + (rushScore / 3 + 1).ToString("00")
            : "Sabah siparişleri";
        DrawText(new Rect(18f, 84f, 245f, 29f), orderHeader, 22, TextAnchor.MiddleLeft, 0, true);
        DrawText(new Rect(257f, 86f, 115f, 25f), selectedMode == GameMode.Story
            ? DistrictName(level)
            : Mathf.CeilToInt(remainingTime) + " sn", 12, TextAnchor.MiddleRight, 1, true);
        if (selectedMode != GameMode.Story)
        {
            float duration = selectedMode == GameMode.Rush ? RushDuration : DailyDuration;
            float progress = Mathf.Clamp01(remainingTime / duration);
            DrawRect(new Rect(18f, 111f, 354f, 4f), new Color(1f, 1f, 1f, 0.14f));
            DrawRect(new Rect(18f, 111f, 354f * progress, 4f), progress < 0.25f ? PackageColors[0] : PackageColors[3]);
        }

        for (int i = 0; i < orders.Count; i++)
        {
            DrawOrder(new Rect(15f, 119f + i * 102f, 360f, 94f), orders[i], i);
        }

        DrawPanel(new Rect(15f, 432f, 360f, 102f), new Color(0.95f, 0.91f, 0.81f), new Color(0.91f, 0.68f, 0.33f));
        DrawText(new Rect(27f, 438f, 178f, 22f), "AYIRMA TEPSİSİ", 13, TextAnchor.MiddleLeft, 1, true);
        DrawText(new Rect(263f, 438f, 98f, 22f), tray.Count + " / 3 PAKET", 12, TextAnchor.MiddleRight, 0, true);

        for (int i = 0; i < 3; i++)
        {
            Rect slot = new Rect(27f + i * 65f, 467f, 59f, 53f);
            int packageColor = i < tray.Count ? packages[tray[i]].color : 0;
            Color slotColor = i < tray.Count && packageColor == -1 ? PackageColors[1] : i < tray.Count ? PackageColors[packageColor] : new Color(0.91f, 0.91f, 0.87f);
            DrawPanel(slot, slotColor,
                new Color(0.83f, 0.83f, 0.78f));
            if (i < tray.Count)
            {
                DrawCircle(new Rect(slot.x + 20f, slot.y + 7f, 19f, 19f), new Color(1f, 1f, 1f, 0.88f));
                string slotLabel = packageColor == -1 ? "JOKER" : PackageColorNames[packageColor];
                DrawText(new Rect(slot.x + 2f, slot.y + 28f, slot.width - 4f, 20f), slotLabel, 9, TextAnchor.MiddleCenter, 0, true);
            }
            else
            {
                DrawCircle(new Rect(slot.x + 20f, slot.y + 7f, 19f, 19f), new Color(0.83f, 0.84f, 0.81f));
                DrawText(new Rect(slot.x + 2f, slot.y + 28f, slot.width - 4f, 20f), "BOŞ", 9, TextAnchor.MiddleCenter, 1, true);
            }
        }

        if (!levelComplete && !runFailed && GUI.Button(new Rect(231f, 469f, 62f, 48f), hintUsed ? "İPUCU\n5" : "İPUCU\nÜCRETSİZ", hintButtonStyle))
        {
            ShowHint();
        }

        if (!levelComplete && !runFailed && tray.Count > 0 && GUI.Button(new Rect(300f, 469f, 61f, 48f), "GERİ AL", secondaryButtonStyle))
        {
            UndoLastPackage();
        }

        int messageColorKey = wrongAnswerUntil > Time.unscaledTime ? 4 : tray.Count == 3 ? 3 : 1;
        DrawText(new Rect(18f, 539f, 354f, 25f), message, 13, TextAnchor.MiddleCenter, messageColorKey, true);
        DrawText(new Rect(18f, 570f, 354f, 20f), "PAKETLER  /  Bir pakete dokun", 12, TextAnchor.MiddleLeft, 1, true);

        for (int i = 0; i < packages.Count; i++)
        {
            int column = i % 3;
            int row = i / 3;
            Rect packageRect = new Rect(15f + column * 122f, 594f + row * 43f, 116f, 40f);
            DrawPackage(packageRect, packages[i], i);
        }

        if (deliveryCelebrationUntil > Time.unscaledTime)
        {
            DrawDeliveryCelebration();
        }

        if (levelComplete)
        {
            DrawCompletionOverlay();
        }
        else if (runFailed)
        {
            DrawFailureOverlay();
        }
    }

    private void DrawOrder(Rect rect, Order order, int orderIndex)
    {
        Color background = order.completed ? new Color(0.84f, 0.92f, 0.82f) : new Color(0.98f, 0.95f, 0.86f);
        DrawPanel(rect, background, order.completed ? new Color(0.42f, 0.68f, 0.48f) : new Color(0.82f, 0.80f, 0.71f));
        DrawCircle(new Rect(rect.x + 11f, rect.y + 10f, 26f, 26f), order.completed ? PackageColors[3] : new Color(0.91f, 0.88f, 0.79f));
        DrawText(new Rect(rect.x + 12f, rect.y + 9f, 24f, 26f), order.completed ? "✓" : (orderIndex + 1).ToString(), 13,
            TextAnchor.MiddleCenter, order.completed ? 2 : 1, true);
        DrawText(new Rect(rect.x + 44f, rect.y + 8f, 185f, 25f), ShopNameForOrder(orderIndex), 14, TextAnchor.MiddleLeft, 0, true);
        DrawText(new Rect(rect.x + 225f, rect.y + 8f, 121f, 25f), order.completed ? "TESLİM EDİLDİ" : "3 PAKET", 11,
            TextAnchor.MiddleRight, order.completed ? 3 : 1, true);

        for (int i = 0; i < order.recipe.Length; i++)
        {
            Rect chip = new Rect(rect.x + 45f + i * 91f, rect.y + 44f, 84f, 37f);
            DrawPanel(chip, PackageColors[order.recipe[i]], PackageColors[order.recipe[i]]);
            DrawRect(new Rect(chip.x + 6f, chip.y + 3f, 7f, chip.height - 6f), new Color(1f, 1f, 1f, 0.28f));
            DrawText(chip, PackageColorNames[order.recipe[i]], 10, TextAnchor.MiddleCenter, 2, true);
        }
    }

    private void DrawPackage(Rect rect, Package package, int packageIndex)
    {
        bool available = !package.used && !tray.Contains(packageIndex);
        bool suggested = available && highlightedPackage == packageIndex && hintExpiresAt > Time.unscaledTime;
        Color fill = available ? new Color(0.99f, 0.97f, 0.90f) : new Color(0.72f, 0.73f, 0.73f);
        Color outline = suggested ? PackageColors[1] : available ? new Color(0.82f, 0.80f, 0.71f) : new Color(0.61f, 0.63f, 0.65f);
        DrawPanel(rect, fill, outline);
        Color packageColor = package.color == -1 ? PackageColors[1] : PackageColors[package.color];
        DrawRect(new Rect(rect.x + 7f, rect.y + 5f, 9f, rect.height - 10f), packageColor);
        DrawRect(new Rect(rect.x + 8f, rect.y + rect.height * 0.45f, 7f, 7f), new Color(1f, 1f, 1f, 0.72f));
        DrawRect(new Rect(rect.x + 2f, rect.y + 6f, 3f, rect.height - 12f), new Color(0f, 0f, 0f, 0.08f));

        if (available && !levelComplete && !runFailed && GUI.Button(rect, GUIContent.none, transparentButtonStyle))
        {
            AddPackage(packageIndex);
        }

        string packageName = package.color == -1 ? "JOKER PAKET" : PackageColorNames[package.color];
        DrawText(new Rect(rect.x + 22f, rect.y + 2f, rect.width - 26f, 16f), suggested ? "ÖNERİLEN PAKET" : "KARGO  /  " + (packageIndex + 1).ToString("00"), 8,
            TextAnchor.MiddleLeft, 1, true);
        DrawText(new Rect(rect.x + 22f, rect.y + 17f, rect.width - 26f, 21f), available ? packageName : "TESLİM EDİLDİ", 9,
            TextAnchor.MiddleLeft, available ? 0 : 1, true);
    }

    private void DrawCompletionOverlay()
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.04f, 0.06f, 0.10f, 0.78f));
        DrawPanel(new Rect(24f, 253f, 342f, 274f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawCircle(new Rect(163f, 271f, 64f, 64f), new Color(0.96f, 0.75f, 0.31f));
        DrawText(new Rect(165f, 273f, 60f, 58f), "✓", 34, TextAnchor.MiddleCenter, 2, true);
        DrawText(new Rect(38f, 337f, 314f, 36f), selectedMode == GameMode.Daily ? "GÜNLÜK ROTA TAMAM!" : "TESLİMAT TAMAM!", 22, TextAnchor.MiddleCenter, 3, true);
        DrawText(new Rect(42f, 373f, 306f, 36f), selectedMode == GameMode.Story
            ? DistrictName(level) + "  •  " + mistakes + " hata"
            : selectedMode == GameMode.Rush ? "En iyi teslimat: " + modeBestScore : "Bugünkü özel rota tamamlandı", 14, TextAnchor.MiddleCenter, 1, true);
        DrawText(new Rect(47f, 405f, 296f, 42f), StarsText(starsAwarded), 34, TextAnchor.MiddleCenter, 5, true);
        DrawText(new Rect(42f, 450f, 306f, 28f), dailyRewardGranted && selectedMode == GameMode.Daily
            ? "Günlük ödülün alındı  •  " + totalStars + " yıldız"
            : "+" + (starsAwarded * 2) + " jeton bonusu  •  " + totalStars + " yıldız toplam", 13, TextAnchor.MiddleCenter, 0, false);

        bool continueToNextLevel = false;
        bool completionReturnToMenu = false;
        if (selectedMode == GameMode.Story)
        {
            continueToNextLevel = GUI.Button(new Rect(37f, 478f, 201f, 40f), "SONRAKİ TESLİMAT", primaryButtonStyle);
            completionReturnToMenu = GUI.Button(new Rect(247f, 478f, 105f, 40f), "MENÜ", secondaryButtonStyle);
        }
        else
        {
            completionReturnToMenu = GUI.Button(new Rect(48f, 478f, 294f, 40f), "ANA MENÜYE DÖN", primaryButtonStyle);
        }

        if (continueToNextLevel)
        {
            level++;
            selectedLevel = level;
            unlockedLevel = Mathf.Max(unlockedLevel, level);
            PlayerPrefs.SetInt("Paketci.UnlockedLevel", unlockedLevel);
            PlayerPrefs.Save();
            levelComplete = false;
            StartLevel();
            PlaySound(tapClip);
        }
        else if (completionReturnToMenu)
        {
            ReturnToMenu();
            PlaySound(tapClip);
        }
    }

    private void DrawPauseOverlay()
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.04f, 0.06f, 0.10f, 0.82f));
        DrawPanel(new Rect(28f, 210f, 334f, 362f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawText(new Rect(45f, 232f, 300f, 46f), "KISA BİR MOLA", 28, TextAnchor.MiddleCenter, 0, true);
        DrawText(new Rect(45f, 278f, 300f, 32f), "Siparişlerin seni bekliyor.", 15, TextAnchor.MiddleCenter, 1, false);

        if (GUI.Button(new Rect(51f, 330f, 288f, 56f), "DEVAM ET", primaryButtonStyle))
        {
            paused = false;
            PlaySound(tapClip);
        }
        if (GUI.Button(new Rect(51f, 397f, 288f, 48f), "AYARLAR", secondaryButtonStyle))
        {
            settingsOpen = true;
            PlaySound(tapClip);
        }
        if (GUI.Button(new Rect(51f, 456f, 288f, 48f), "BÖLÜMÜ YENİDEN BAŞLAT", secondaryButtonStyle))
        {
            StartLevel();
            paused = false;
            PlaySound(tapClip);
        }
        if (GUI.Button(new Rect(51f, 515f, 288f, 42f), "ANA MENÜYE DÖN", secondaryButtonStyle))
        {
            ReturnToMenu();
            PlaySound(tapClip);
        }
    }

    private void DrawSettingsOverlay(bool inGame)
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.04f, 0.06f, 0.10f, 0.84f));
        DrawPanel(new Rect(25f, 212f, 340f, 355f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawText(new Rect(45f, 230f, 300f, 43f), "AYARLAR", 27, TextAnchor.MiddleCenter, 0, true);

        DrawText(new Rect(47f, 298f, 205f, 34f), "Ses efektleri", 16, TextAnchor.MiddleLeft, 0, true);
        if (GUI.Button(new Rect(259f, 296f, 78f, 38f), soundEnabled ? "AÇIK" : "KAPALI", secondaryButtonStyle))
        {
            soundEnabled = !soundEnabled;
            PlayerPrefs.SetInt("Paketci.Sound", soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
            PlaySound(tapClip);
        }

        DrawText(new Rect(47f, 349f, 196f, 34f), "Ses seviyesi", 16, TextAnchor.MiddleLeft, 0, true);
        float volume = GUI.HorizontalSlider(new Rect(245f, 357f, 90f, 20f), masterVolume, 0f, 1f);
        if (!Mathf.Approximately(volume, masterVolume))
        {
            masterVolume = volume;
            if (uiAudioSource != null)
            {
                uiAudioSource.volume = masterVolume;
            }
            PlayerPrefs.SetFloat("Paketci.Volume", masterVolume);
        }
        DrawText(new Rect(245f, 375f, 90f, 20f), Mathf.RoundToInt(masterVolume * 100f) + "%", 11, TextAnchor.MiddleCenter, 1, false);

        DrawText(new Rect(47f, 411f, 205f, 34f), "Titreşim", 16, TextAnchor.MiddleLeft, 0, true);
        if (GUI.Button(new Rect(259f, 409f, 78f, 38f), hapticsEnabled ? "AÇIK" : "KAPALI", secondaryButtonStyle))
        {
            hapticsEnabled = !hapticsEnabled;
            PlayerPrefs.SetInt("Paketci.Haptics", hapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            Vibrate();
            PlaySound(tapClip);
        }

        DrawText(new Rect(48f, 460f, 288f, 35f), "İlerlemen otomatik kaydedilir.", 13, TextAnchor.MiddleCenter, 1, false);
        string closeLabel = inGame ? "MOLAYA DÖN" : "MENÜYE DÖN";
        if (GUI.Button(new Rect(49f, 505f, 290f, 43f), closeLabel, primaryButtonStyle))
        {
            settingsOpen = false;
            PlayerPrefs.Save();
            PlaySound(tapClip);
        }
    }

    private void DrawLevelSelectOverlay()
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.04f, 0.06f, 0.10f, 0.88f));
        DrawPanel(new Rect(20f, 150f, 350f, 480f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawText(new Rect(42f, 166f, 306f, 40f), "BÖLÜM HARİTASI", 24, TextAnchor.MiddleCenter, 0, true);
        DrawText(new Rect(42f, 207f, 306f, 25f), DistrictName(selectedLevel), 14, TextAnchor.MiddleCenter, 3, true);

        int pageCount = Mathf.Max(1, (unlockedLevel + 11) / 12);
        levelPage = Mathf.Clamp(levelPage, 0, pageCount - 1);
        if (GUI.Button(new Rect(42f, 241f, 58f, 38f), "‹", secondaryButtonStyle))
        {
            levelPage = Mathf.Max(0, levelPage - 1);
            PlaySound(tapClip);
        }
        DrawText(new Rect(110f, 243f, 170f, 34f), "SAYFA " + (levelPage + 1) + " / " + pageCount, 13, TextAnchor.MiddleCenter, 1, true);
        if (GUI.Button(new Rect(290f, 241f, 58f, 38f), "›", secondaryButtonStyle))
        {
            levelPage = Mathf.Min(pageCount - 1, levelPage + 1);
            PlaySound(tapClip);
        }

        for (int i = 0; i < 12; i++)
        {
            int levelNumber = levelPage * 12 + i + 1;
            int column = i % 4;
            int row = i / 4;
            Rect buttonRect = new Rect(41f + column * 78f, 297f + row * 77f, 68f, 61f);
            bool unlocked = levelNumber <= unlockedLevel;
            Color fill = levelNumber == selectedLevel ? PackageColors[1] : unlocked
                ? new Color(0.88f, 0.91f, 0.84f)
                : new Color(0.84f, 0.83f, 0.78f);
            DrawPanel(buttonRect, fill, unlocked ? new Color(0.74f, 0.76f, 0.68f) : new Color(0.76f, 0.75f, 0.70f));
            if (unlocked && GUI.Button(buttonRect, GUIContent.none, transparentButtonStyle))
            {
                selectedLevel = levelNumber;
                levelSelectOpen = false;
                PlaySound(tapClip);
            }
            if (unlocked)
            {
                DrawText(new Rect(buttonRect.x, buttonRect.y + 5f, buttonRect.width, 26f), levelNumber.ToString("00"), 18,
                    TextAnchor.MiddleCenter, 0, true);
                int earnedStars = PlayerPrefs.GetInt(LevelStarsKey(levelNumber), 0);
                DrawText(new Rect(buttonRect.x, buttonRect.y + 32f, buttonRect.width, 20f), StarsText(earnedStars), 10,
                    TextAnchor.MiddleCenter, 5, true);
            }
            else
            {
                DrawText(buttonRect, "KİLİTLİ", 8, TextAnchor.MiddleCenter, 1, true);
            }
        }

        DrawText(new Rect(42f, 540f, 306f, 28f), "Açık bölüm: " + unlockedLevel + " / " + DistrictName(unlockedLevel), 12,
            TextAnchor.MiddleCenter, 1, false);
        if (GUI.Button(new Rect(49f, 578f, 290f, 40f), "KAPAT", primaryButtonStyle))
        {
            levelSelectOpen = false;
            PlaySound(tapClip);
        }
    }

    private void DrawFailureOverlay()
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, DesignHeight), new Color(0.04f, 0.06f, 0.10f, 0.84f));
        DrawPanel(new Rect(25f, 262f, 340f, 258f), new Color(0.98f, 0.95f, 0.86f), new Color(0.91f, 0.68f, 0.33f));
        DrawText(new Rect(43f, 285f, 304f, 48f), selectedMode == GameMode.Daily ? "SÜRE DOLDU" : "ROTA SONA ERDİ", 25, TextAnchor.MiddleCenter, 4, true);
        int delivered = selectedMode == GameMode.Rush ? rushScore : orders.FindAll(order => order.completed).Count;
        DrawText(new Rect(43f, 337f, 304f, 49f), "Bu kez " + delivered + " sipariş teslim ettin.\nBir tur daha denemek ister misin?", 16,
            TextAnchor.MiddleCenter, 0, false);
        DrawText(new Rect(43f, 391f, 304f, 29f), "EN İYİ: " + modeBestScore + " SİPARİŞ", 13, TextAnchor.MiddleCenter, 1, true);

        if (GUI.Button(new Rect(48f, 434f, 294f, 43f), "YENİDEN DENE", primaryButtonStyle))
        {
            StartLevel();
            PlaySound(tapClip);
        }
        if (GUI.Button(new Rect(48f, 483f, 294f, 34f), "ANA MENÜYE DÖN", secondaryButtonStyle))
        {
            ReturnToMenu();
            PlaySound(tapClip);
        }
    }

    private void StartLevel()
    {
        packages.Clear();
        orders.Clear();
        tray.Clear();
        levelComplete = false;
        runFailed = false;
        paused = false;
        hintUsed = false;
        highlightedPackage = -1;
        mistakes = 0;
        deliveryStreak = 0;
        lastDeliveryReward = 0;
        rushScore = 0;
        starsAwarded = 0;
        message = "Üç paketi tepsiye koyup bir siparişi tamamla.";
        RefreshDailyRewardState();

        int seed = level * 7919;
        if (selectedMode == GameMode.Rush)
        {
            seed = Environment.TickCount;
            remainingTime = RushDuration;
        }
        else if (selectedMode == GameMode.Daily)
        {
            DateTime utcDate = DateTime.UtcNow;
            seed = utcDate.Year * 10000 + utcDate.Month * 100 + utcDate.Day;
            remainingTime = DailyDuration;
        }
        else
        {
            remainingTime = 0f;
        }

        int colorCount = selectedMode == GameMode.Daily ? 4 : ColorCountForLevel(level);
        BuildOrderWave(seed, colorCount, selectedMode != GameMode.Daily && level >= 3);
    }

    private void StartRushWave()
    {
        int difficultyLevel = 1 + rushScore / 3;
        int seed = Environment.TickCount ^ (rushScore * 7919);
        BuildOrderWave(seed, ColorCountForLevel(difficultyLevel), difficultyLevel >= 3);
        hintUsed = false;
        message = "Yeni rota! Siparişleri seri teslim et.";
    }

    private void BuildOrderWave(int seed, int colorCount, bool addJoker)
    {
        packages.Clear();
        orders.Clear();
        tray.Clear();

        System.Random random = new System.Random(seed);
        List<int> supply = new List<int>(9);
        for (int orderIndex = 0; orderIndex < 3; orderIndex++)
        {
            int[] recipe = new int[3];
            do
            {
                for (int i = 0; i < recipe.Length; i++)
                {
                    recipe[i] = random.Next(colorCount);
                }
            }
            while (CountDistinctColors(recipe) < 2);

            orders.Add(new Order(recipe));
            supply.AddRange(recipe);
        }

        if (addJoker)
        {
            supply.Add(-1);
        }

        for (int i = supply.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            int temporary = supply[i];
            supply[i] = supply[swapIndex];
            supply[swapIndex] = temporary;
        }

        for (int i = 0; i < supply.Count; i++)
        {
            packages.Add(new Package(supply[i]));
        }
    }

    private void AddPackage(int packageIndex)
    {
        if (tray.Count >= 3 || packages[packageIndex].used || tray.Contains(packageIndex))
        {
            return;
        }

        tray.Add(packageIndex);
        if (tray.Count < 3)
        {
            message = "Bir paket daha seç.";
            return;
        }

        int[] pickedColors = new int[3];
        for (int i = 0; i < tray.Count; i++)
        {
            pickedColors[i] = packages[tray[i]].color;
        }

        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].completed || !CanMatchOrder(pickedColors, orders[i].recipe))
            {
                continue;
            }

            orders[i].completed = true;
            for (int package = 0; package < tray.Count; package++)
            {
                packages[tray[package]].used = true;
            }

            tray.Clear();
            deliveryStreak++;
            int deliveryReward = 5 + Mathf.Min(deliveryStreak - 1, 3);
            lastDeliveryReward = selectedMode == GameMode.Daily ? 0 : deliveryReward;
            if (selectedMode != GameMode.Daily)
            {
                coins += deliveryReward;
                PlayerPrefs.SetInt("Paketci.Coins", coins);
                PlayerPrefs.Save();
            }
            message = deliveryStreak > 1
                ? "Seri x" + deliveryStreak + "! Sipariş teslim edildi."
                : "Doğru paketler! Sipariş teslim edildi.";
            deliveryCelebrationUntil = Time.unscaledTime + 1.35f;
            wrongAnswerUntil = 0f;
            highlightedPackage = -1;
            PlaySound(successClip);
            Vibrate();
            if (selectedMode == GameMode.Rush)
            {
                remainingTime = Mathf.Min(RushDuration, remainingTime + 8f);
                rushScore++;
                modeBestScore = Mathf.Max(modeBestScore, rushScore);
                PlayerPrefs.SetInt("Paketci.RushBest", modeBestScore);
            }
            if (AllOrdersComplete())
            {
                if (selectedMode == GameMode.Rush)
                {
                    PlayerPrefs.SetInt("Paketci.Coins", coins);
                    PlayerPrefs.Save();
                    StartRushWave();
                }
                else
                {
                    levelComplete = true;
                    starsAwarded = mistakes == 0 ? 3 : mistakes <= 2 ? 2 : 1;
                    if (selectedMode == GameMode.Daily)
                    {
                        if (!dailyRewardGranted)
                        {
                            totalStars += starsAwarded;
                            coins += 10;
                            dailyRewardGranted = true;
                            PlayerPrefs.SetString("Paketci.DailyRewardDate", CurrentDateKey());
                        }
                        else
                        {
                            starsAwarded = 0;
                        }
                    }
                    else if (selectedMode == GameMode.Story)
                    {
                        string starsKey = LevelStarsKey(level);
                        int previousBest = PlayerPrefs.GetInt(starsKey, 0);
                        totalStars += Mathf.Max(0, starsAwarded - previousBest);
                        PlayerPrefs.SetInt(starsKey, Mathf.Max(previousBest, starsAwarded));
                        coins += starsAwarded * 2;
                    }

                    if (selectedMode == GameMode.Story)
                    {
                        unlockedLevel = Mathf.Max(unlockedLevel, level + 1);
                        selectedLevel = Mathf.Max(selectedLevel, unlockedLevel);
                        PlayerPrefs.SetInt("Paketci.UnlockedLevel", unlockedLevel);
                    }
                    PlayerPrefs.SetInt("Paketci.Coins", coins);
                    PlayerPrefs.SetInt("Paketci.Stars", totalStars);
                    PlayerPrefs.Save();
                }
            }
            return;
        }

        message = "Bu üç paket siparişle eşleşmedi. Geri alıp yeniden dene.";
        mistakes++;
        deliveryStreak = 0;
        wrongAnswerUntil = Time.unscaledTime + 1.25f;
        PlaySound(errorClip);
        Vibrate();
        if (selectedMode == GameMode.Rush)
        {
            remainingTime = Mathf.Max(0f, remainingTime - 5f);
            if (remainingTime <= 0f)
            {
                FinishTimedRun();
            }
        }
    }

    private void UndoLastPackage()
    {
        if (tray.Count == 0)
        {
            return;
        }

        tray.RemoveAt(tray.Count - 1);
        message = "Son paket geri alındı. Başka bir renk seç.";
    }

    private void ShowHint()
    {
        if (levelComplete)
        {
            return;
        }

        if (hintUsed && coins < 5)
        {
            message = "İpucu için 5 jeton gerekiyor.";
            return;
        }

        for (int orderIndex = 0; orderIndex < orders.Count; orderIndex++)
        {
            if (orders[orderIndex].completed)
            {
                continue;
            }

            int[] needed = new int[PackageColors.Length];
            int jokersInTray = 0;
            for (int i = 0; i < orders[orderIndex].recipe.Length; i++)
            {
                needed[orders[orderIndex].recipe[i]]++;
            }

            bool matchesTray = true;
            for (int i = 0; i < tray.Count; i++)
            {
                int selectedColor = packages[tray[i]].color;
                if (selectedColor == -1)
                {
                    jokersInTray++;
                    continue;
                }
                needed[selectedColor]--;
                if (needed[selectedColor] < 0)
                {
                    matchesTray = false;
                }
            }

            if (!matchesTray)
            {
                continue;
            }

            int remainingSlots = 0;
            for (int color = 0; color < needed.Length; color++)
            {
                remainingSlots += Mathf.Max(0, needed[color]);
            }
            if (remainingSlots <= jokersInTray)
            {
                message = "Joker paket eksik rengi tamamlar; diğer paketleri seç.";
                return;
            }

            for (int color = 0; color < needed.Length; color++)
            {
                if (needed[color] <= 0)
                {
                    continue;
                }

                for (int package = 0; package < packages.Count; package++)
                {
                    if (!packages[package].used && !tray.Contains(package) && packages[package].color == color)
                    {
                        if (hintUsed)
                        {
                            coins -= 5;
                            PlayerPrefs.SetInt("Paketci.Coins", coins);
                            PlayerPrefs.Save();
                        }

                        hintUsed = true;
                        highlightedPackage = package;
                        hintExpiresAt = Time.unscaledTime + 3f;
                        message = "Işıklı paket, açık siparişlerden birine uygun.";
                        return;
                    }
                }
            }
        }

        message = "Önce tepsideki paketlerden birini geri al.";
    }

    private bool AllOrdersComplete()
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (!orders[i].completed)
            {
                return false;
            }
        }
        return true;
    }

    private static bool CanMatchOrder(int[] selectedColors, int[] recipe)
    {
        int[] needed = new int[PackageColors.Length];
        for (int i = 0; i < recipe.Length; i++)
        {
            needed[recipe[i]]++;
        }

        int jokerCount = 0;
        for (int i = 0; i < selectedColors.Length; i++)
        {
            int color = selectedColors[i];
            if (color == -1)
            {
                jokerCount++;
                continue;
            }
            needed[color]--;
            if (needed[color] < 0)
            {
                return false;
            }
        }

        int remainingColors = 0;
        for (int i = 0; i < needed.Length; i++)
        {
            remainingColors += needed[i];
        }
        return remainingColors == jokerCount;
    }

    private static int CountDistinctColors(int[] colors)
    {
        bool[] found = new bool[PackageColors.Length];
        int count = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            if (!found[colors[i]])
            {
                found[colors[i]] = true;
                count++;
            }
        }
        return count;
    }

    private void DrawBackdrop()
    {
        DrawRect(new Rect(0f, 0f, DesignWidth, 10f), new Color(0.95f, 0.69f, 0.30f));
        DrawRect(new Rect(0f, 10f, DesignWidth, 1f), new Color(1f, 1f, 1f, 0.12f));
        DrawCircle(new Rect(324f, 68f, 28f, 28f), new Color(0.97f, 0.74f, 0.36f, 0.28f));
        DrawCircle(new Rect(338f, 88f, 8f, 8f), new Color(0.98f, 0.95f, 0.86f, 0.26f));
        DrawCircle(new Rect(42f, 74f, 5f, 5f), new Color(0.98f, 0.95f, 0.86f, 0.28f));
        DrawCircle(new Rect(72f, 50f, 4f, 4f), new Color(0.98f, 0.95f, 0.86f, 0.23f));
    }

    private void DrawTown()
    {
        DrawCircle(new Rect(286f, 97f, 54f, 54f), new Color(0.98f, 0.76f, 0.39f, 0.78f));
        DrawRect(new Rect(0f, 676f, DesignWidth, 104f), new Color(0.14f, 0.19f, 0.28f));
        DrawRect(new Rect(0f, 744f, DesignWidth, 36f), new Color(0.10f, 0.14f, 0.22f));

        DrawBuilding(new Rect(8f, 615f, 62f, 78f), new Color(0.24f, 0.34f, 0.44f), 3);
        DrawBuilding(new Rect(78f, 592f, 76f, 101f), new Color(0.29f, 0.39f, 0.46f), 4);
        DrawBuilding(new Rect(245f, 620f, 60f, 73f), new Color(0.24f, 0.34f, 0.44f), 3);
        DrawBuilding(new Rect(313f, 586f, 68f, 107f), new Color(0.29f, 0.39f, 0.46f), 4);

        DrawRect(new Rect(0f, 691f, DesignWidth, 5f), new Color(0.91f, 0.55f, 0.33f));
        DrawRect(new Rect(16f, 721f, 56f, 4f), new Color(0.96f, 0.83f, 0.55f, 0.7f));
        DrawRect(new Rect(123f, 721f, 56f, 4f), new Color(0.96f, 0.83f, 0.55f, 0.7f));
        DrawRect(new Rect(231f, 721f, 56f, 4f), new Color(0.96f, 0.83f, 0.55f, 0.7f));
        DrawRect(new Rect(338f, 721f, 36f, 4f), new Color(0.96f, 0.83f, 0.55f, 0.7f));
    }

    private void DrawBuilding(Rect rect, Color color, int windowRows)
    {
        DrawRect(rect, new Color(0.10f, 0.14f, 0.21f, 0.55f));
        DrawRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 3f), color);
        for (int row = 0; row < windowRows; row++)
        {
            for (int column = 0; column < 2; column++)
            {
                Color window = (row + column) % 3 == 0 ? new Color(0.94f, 0.69f, 0.39f, 0.72f) : new Color(0.62f, 0.73f, 0.76f, 0.50f);
                DrawRect(new Rect(rect.x + 11f + column * 25f, rect.y + 13f + row * 17f, 9f, 10f), window);
            }
        }
    }

    private void DrawMascot(Rect rect)
    {
        float bob = Mathf.Sin(Time.unscaledTime * 2.4f) * 2f;
        Rect box = new Rect(rect.x + 9f, rect.y + 8f + bob, rect.width - 18f, rect.height - 13f);
        DrawPanel(new Rect(box.x - 4f, box.y + 5f, box.width + 8f, box.height), new Color(0.92f, 0.53f, 0.29f), new Color(0.98f, 0.76f, 0.39f));
        DrawRect(new Rect(box.center.x - 5f, box.y + 2f, 10f, box.height - 4f), new Color(0.98f, 0.78f, 0.44f));
        DrawCircle(new Rect(box.x + 16f, box.y + 24f, 7f, 7f), new Color(0.13f, 0.18f, 0.27f));
        DrawCircle(new Rect(box.x + box.width - 23f, box.y + 24f, 7f, 7f), new Color(0.13f, 0.18f, 0.27f));
        DrawRect(new Rect(box.x + 20f, box.y + 42f, box.width - 40f, 3f), new Color(0.13f, 0.18f, 0.27f));
    }

    private void DrawStatCard(Rect rect, string title, string value, Color accent)
    {
        DrawPanel(rect, new Color(0.98f, 0.95f, 0.86f), new Color(0.82f, 0.80f, 0.71f));
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 4f), accent);
        DrawText(new Rect(rect.x + 5f, rect.y + 12f, rect.width - 10f, 19f), title, 11, TextAnchor.MiddleCenter, 1, true);
        DrawText(new Rect(rect.x + 5f, rect.y + 30f, rect.width - 10f, 32f), value, 23, TextAnchor.MiddleCenter, 0, true);
    }

    private void DrawCircle(Rect rect, Color color)
    {
        if (circleTexture == null)
        {
            const int size = 64;
            circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f));
                    float alpha = Mathf.Clamp01((size * 0.5f - distance) * 0.8f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            circleTexture.SetPixels(pixels);
            circleTexture.Apply();
            generatedTextures.Add(circleTexture);
        }

        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, circleTexture);
        GUI.color = previousColor;
    }

    private void DrawDeliveryCelebration()
    {
        float remaining = Mathf.Clamp01((deliveryCelebrationUntil - Time.unscaledTime) / 1.35f);
        float rise = (1f - remaining) * 25f;
        string rewardText = selectedMode == GameMode.Daily ? "TESLİM!  ✦  GÜZEL İŞ" : "+" + lastDeliveryReward + " JETON  ✦  TESLİM!";
        DrawText(new Rect(72f, 393f - rise, 246f, 32f), rewardText, 15, TextAnchor.MiddleCenter, 5, true);
        float sparkle = 0.65f + Mathf.Sin(Time.unscaledTime * 12f) * 0.25f;
        DrawRect(new Rect(79f, 403f - rise, 7f, 7f), new Color(1f, 0.80f, 0.38f, sparkle));
        DrawRect(new Rect(300f, 409f - rise, 6f, 6f), new Color(1f, 0.80f, 0.38f, sparkle));
    }

    private static string ShopNameForOrder(int orderIndex)
    {
        string[] names = { "Fırıncı Nermin", "Çiçekçi Lale", "Oyuncakçı Ali" };
        return names[orderIndex % names.Length];
    }

    private static string StarsText(int count)
    {
        return new string('★', count) + new string('☆', 3 - count);
    }

    private static string LevelStarsKey(int levelNumber)
    {
        return "Paketci.LevelStars." + levelNumber;
    }

    private static int ColorCountForLevel(int levelNumber)
    {
        if (levelNumber >= 13)
        {
            return 6;
        }
        if (levelNumber >= 8)
        {
            return 5;
        }
        if (levelNumber >= 4)
        {
            return 4;
        }
        return 3;
    }

    private static int NextColorUnlockLevel(int currentColorCount)
    {
        if (currentColorCount == 3)
        {
            return 4;
        }
        if (currentColorCount == 4)
        {
            return 8;
        }
        return 13;
    }

    private void EnsureStyles()
    {
        if (stylesReady)
        {
            return;
        }

        primaryButtonStyle = CreateButtonStyle(new Color(0.91f, 0.55f, 0.23f), new Color(0.96f, 0.63f, 0.29f), new Color(0.80f, 0.44f, 0.17f), 19);
        secondaryButtonStyle = CreateButtonStyle(new Color(0.24f, 0.34f, 0.48f), new Color(0.30f, 0.42f, 0.58f), new Color(0.17f, 0.26f, 0.39f), 13);
        hintButtonStyle = CreateButtonStyle(new Color(0.22f, 0.55f, 0.39f), new Color(0.27f, 0.63f, 0.44f), new Color(0.17f, 0.45f, 0.31f), 10);
        transparentButtonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = CreateTexture(Color.clear), textColor = Color.clear },
            hover = { background = CreateTexture(new Color(1f, 1f, 1f, 0.08f)), textColor = Color.clear },
            active = { background = CreateTexture(new Color(1f, 1f, 1f, 0.15f)), textColor = Color.clear },
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0)
        };
        stylesReady = true;
    }

    private GUIStyle CreateButtonStyle(Color normal, Color hover, Color active, int fontSize)
    {
        GUIStyle style = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = CreateTexture(normal), textColor = TextColors[2] },
            hover = { background = CreateTexture(hover), textColor = TextColors[2] },
            active = { background = CreateTexture(active), textColor = TextColors[2] },
            border = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(8, 8, 5, 5)
        };
        return style;
    }

    private GUIStyle GetTextStyle(int fontSize, TextAnchor alignment, int colorKey, bool bold)
    {
        int key = ((fontSize * 10 + (int)alignment) * 20) + colorKey * 2 + (bold ? 1 : 0);
        if (!textStyles.TryGetValue(key, out GUIStyle style))
        {
            style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                alignment = alignment,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                wordWrap = true,
                normal = { textColor = TextColors[colorKey] }
            };
            textStyles.Add(key, style);
        }
        return style;
    }

    private void DrawText(Rect rect, string text, int fontSize, TextAnchor alignment, int colorKey, bool bold)
    {
        GUI.Label(rect, text, GetTextStyle(fontSize, alignment, colorKey, bold));
    }

    private void DrawPanel(Rect rect, Color fill, Color outline)
    {
        DrawRect(new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.16f));
        DrawRect(rect, outline);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), fill);
    }

    private static void DrawRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Point
        };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        generatedTextures.Add(texture);
        return texture;
    }

    private void CreateAudioFeedback()
    {
        uiAudioSource = gameObject.AddComponent<AudioSource>();
        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
        uiAudioSource.spatialBlend = 0f;
        uiAudioSource.volume = masterVolume;
        tapClip = CreateTone("Paketci_Tap", 720f, 0.045f);
        successClip = CreateTone("Paketci_Success", 980f, 0.13f);
        errorClip = CreateTone("Paketci_Error", 260f, 0.11f);
    }

    private AudioClip CreateTone(string clipName, float frequency, float duration)
    {
        const int sampleRate = 22050;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < samples.Length; i++)
        {
            float envelope = 1f - (float)i / samples.Length;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * envelope * 0.18f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        generatedAudioClips.Add(clip);
        return clip;
    }

    private void PlaySound(AudioClip clip)
    {
        if (soundEnabled && uiAudioSource != null && clip != null)
        {
            uiAudioSource.volume = masterVolume;
            uiAudioSource.PlayOneShot(clip);
        }
    }

    private void Vibrate()
    {
        if (hapticsEnabled)
        {
            Handheld.Vibrate();
        }
    }

    private sealed class Package
    {
        public readonly int color;
        public bool used;

        public Package(int color)
        {
            this.color = color;
        }
    }

    private sealed class Order
    {
        public readonly int[] recipe;
        public bool completed;

        public Order(int[] recipe)
        {
            this.recipe = recipe;
        }
    }
}
