using System.Collections.Generic;
using UnityEngine;

// Progress kept between runs, stored in PlayerPrefs.
public static class SaveData
{
    private const string CoinsKey = "coins";
    private const string BestTimeKey = "best_time";
    private const string BestKillsKey = "best_kills";
    private const string CharacterKey = "character";
    private const string MusicVolumeKey = "music_volume";
    private const string SfxVolumeKey = "sfx_volume";
    private const string MetaKeyPrefix = "meta_";

    // Simulation runs keep everything in memory: they start from a fresh profile and never
    // touch (or race on) the player's real saved data
    public static bool InMemory { get; set; }
    private static readonly Dictionary<string, object> s_Memory = new Dictionary<string, object>();

    public static int Coins
    {
        get => GetInt(CoinsKey, 0);
        set => SetInt(CoinsKey, Mathf.Max(0, value));
    }

    public static float BestTime
    {
        get => GetFloat(BestTimeKey, 0f);
        set => SetFloat(BestTimeKey, value);
    }

    public static int BestKills
    {
        get => GetInt(BestKillsKey, 0);
        set => SetInt(BestKillsKey, value);
    }

    // Asset name of the chosen CharacterData; empty means the scene default
    public static string SelectedCharacter
    {
        get => GetString(CharacterKey, "");
        set => SetString(CharacterKey, value);
    }

    public static float GetMusicVolume(float defaultValue) => GetFloat(MusicVolumeKey, defaultValue);
    public static void SetMusicVolume(float value) => SetFloat(MusicVolumeKey, value);
    public static float GetSfxVolume(float defaultValue) => GetFloat(SfxVolumeKey, defaultValue);
    public static void SetSfxVolume(float value) => SetFloat(SfxVolumeKey, value);

    public static int GetMetaLevel(MetaStat stat) => GetInt(MetaKeyPrefix + stat, 0);

    public static float MetaBonus(MetaStat stat) => MetaUpgrades.Bonus(stat, GetMetaLevel(stat));

    public static bool TryBuyMeta(MetaStat stat)
    {
        int level = GetMetaLevel(stat);
        if (level >= MetaUpgrades.MaxLevel) return false;
        int cost = MetaUpgrades.Cost(level);
        if (Coins < cost) return false;
        Coins -= cost;
        SetInt(MetaKeyPrefix + stat, level + 1);
        return true;
    }

    private static int GetInt(string key, int defaultValue)
    {
        if (!InMemory) return PlayerPrefs.GetInt(key, defaultValue);
        return s_Memory.TryGetValue(key, out object value) ? (int)value : defaultValue;
    }

    private static float GetFloat(string key, float defaultValue)
    {
        if (!InMemory) return PlayerPrefs.GetFloat(key, defaultValue);
        return s_Memory.TryGetValue(key, out object value) ? (float)value : defaultValue;
    }

    private static string GetString(string key, string defaultValue)
    {
        if (!InMemory) return PlayerPrefs.GetString(key, defaultValue);
        return s_Memory.TryGetValue(key, out object value) ? (string)value : defaultValue;
    }

    private static void SetInt(string key, int value)
    {
        if (InMemory) { s_Memory[key] = value; return; }
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    private static void SetFloat(string key, float value)
    {
        if (InMemory) { s_Memory[key] = value; return; }
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private static void SetString(string key, string value)
    {
        if (InMemory) { s_Memory[key] = value; return; }
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }
}
