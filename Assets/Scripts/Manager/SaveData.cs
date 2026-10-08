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

    public static int Coins
    {
        get => PlayerPrefs.GetInt(CoinsKey, 0);
        set => SetInt(CoinsKey, Mathf.Max(0, value));
    }

    public static float BestTime
    {
        get => PlayerPrefs.GetFloat(BestTimeKey, 0f);
        set => SetFloat(BestTimeKey, value);
    }

    public static int BestKills
    {
        get => PlayerPrefs.GetInt(BestKillsKey, 0);
        set => SetInt(BestKillsKey, value);
    }

    // Asset name of the chosen CharacterData; empty means the scene default
    public static string SelectedCharacter
    {
        get => PlayerPrefs.GetString(CharacterKey, "");
        set { PlayerPrefs.SetString(CharacterKey, value); PlayerPrefs.Save(); }
    }

    public static float GetMusicVolume(float defaultValue) => PlayerPrefs.GetFloat(MusicVolumeKey, defaultValue);
    public static void SetMusicVolume(float value) => SetFloat(MusicVolumeKey, value);
    public static float GetSfxVolume(float defaultValue) => PlayerPrefs.GetFloat(SfxVolumeKey, defaultValue);
    public static void SetSfxVolume(float value) => SetFloat(SfxVolumeKey, value);

    public static int GetMetaLevel(MetaStat stat) => PlayerPrefs.GetInt(MetaKeyPrefix + stat, 0);

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

    private static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    private static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}
