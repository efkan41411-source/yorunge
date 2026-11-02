using System;
using System.Collections.Generic;

public sealed class SortingRules
{
    public sealed class Package { public int Color { get; } public bool Used { get; internal set; } public Package(int color) { Color = color; } }
    public sealed class Order { public int[] Recipe { get; } public bool Completed { get; internal set; } public Order(int[] recipe) { Recipe = recipe; } }
    private readonly List<Package> packages = new List<Package>();
    private readonly List<Order> orders = new List<Order>();
    private readonly List<int> tray = new List<int>(3);
    public IReadOnlyList<Package> Packages => packages;
    public IReadOnlyList<Order> Orders => orders;
    public IReadOnlyList<int> Tray => tray;
    public int CompletedCount { get { int count = 0; foreach (var order in orders) if (order.Completed) count++; return count; } }
    public void Build(int seed, int colorCount, bool joker, int decoys)
    {
        packages.Clear(); orders.Clear(); tray.Clear();
        var random = new Random(seed); var supply = new List<int>();
        for (int o = 0; o < 3; o++)
        {
            int[] recipe = new int[3];
            do { for (int i = 0; i < 3; i++) recipe[i] = random.Next(Math.Max(2, Math.Min(6, colorCount))); }
            while (recipe[0] == recipe[1] && recipe[1] == recipe[2]);
            orders.Add(new Order(recipe)); supply.AddRange(recipe);
        }
        if (joker) supply.Add(-1);
        for (int i = 0; i < Math.Min(2, Math.Max(0, decoys)); i++) supply.Add(random.Next(colorCount));
        for (int i = supply.Count - 1; i > 0; i--) { int j = random.Next(i + 1); int temp = supply[i]; supply[i] = supply[j]; supply[j] = temp; }
        foreach (int color in supply) packages.Add(new Package(color));
    }
    public bool Available(int index) => index >= 0 && index < packages.Count && !packages[index].Used && !tray.Contains(index);
    // -2 ignored, -1 incomplete, 0 mismatch, 1 delivery.
    public int Pick(int index)
    {
        if (!Available(index) || tray.Count >= 3) return -2;
        tray.Add(index);
        if (tray.Count < 3) return -1;
        int[] colors = { packages[tray[0]].Color, packages[tray[1]].Color, packages[tray[2]].Color };
        foreach (var order in orders)
        {
            if (order.Completed || !CanMatch(colors, order.Recipe)) continue;
            order.Completed = true; foreach (int p in tray) packages[p].Used = true; tray.Clear(); return 1;
        }
        return 0;
    }
    public void Undo() { if (tray.Count > 0) tray.RemoveAt(tray.Count - 1); }
    public int Hint()
    {
        foreach (var order in orders)
        {
            if (order.Completed) continue;
            int[] needed = new int[6]; foreach (int color in order.Recipe) needed[color]++;
            bool valid = true;
            foreach (int p in tray) if (packages[p].Color >= 0 && --needed[packages[p].Color] < 0) valid = false;
            if (!valid) continue;
            for (int c = 0; c < 6; c++) if (needed[c] > 0)
                for (int p = 0; p < packages.Count; p++) if (Available(p) && packages[p].Color == c) return p;
        }
        return -1;
    }
    public static bool CanMatch(int[] selected, int[] recipe)
    {
        if (selected == null || recipe == null || selected.Length != 3 || recipe.Length != 3) return false;
        int[] needed = new int[6];
        foreach (int color in recipe) { if (color < 0 || color >= 6) return false; needed[color]++; }
        int jokers = 0;
        foreach (int color in selected) { if (color == -1) jokers++; else if (color < 0 || color >= 6 || --needed[color] < 0) return false; }
        int remaining = 0; foreach (int count in needed) remaining += count;
        return remaining == jokers;
    }
}
