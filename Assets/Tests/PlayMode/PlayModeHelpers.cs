using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// Shared by the PlayMode tests. Game types live in Assembly-CSharp, which test assemblies
// can't reference, so they are found and driven by name through reflection.
public static class PlayModeHelpers
{
    public const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static IEnumerator LoadScene(string name)
    {
        AsyncOperation load = SceneManager.LoadSceneAsync(name);
        while (!load.isDone) yield return null;
        yield return null;
    }

    public static IEnumerator WaitRealtime(float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end) yield return null;
    }

    public static Component FindByTypeName(string typeName)
    {
        foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            if (behaviour.GetType().Name == typeName)
                return behaviour;
        return null;
    }

    public static object Invoke(object target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method, AnyInstance);
        Assert.IsNotNull(info, $"{target.GetType().Name}.{method} not found");
        return info.Invoke(target, args);
    }

    public static object GetProperty(object target, string property)
    {
        PropertyInfo info = target.GetType().GetProperty(property, AnyInstance);
        Assert.IsNotNull(info, $"{target.GetType().Name}.{property} not found");
        return info.GetValue(target);
    }

    public static object GetField(object target, string field)
    {
        FieldInfo info = target.GetType().GetField(field, AnyInstance);
        Assert.IsNotNull(info, $"{target.GetType().Name}.{field} not found");
        return info.GetValue(target);
    }

    public static bool IsGameOver(Component game) => (bool)GetField(game, "m_IsGameOver");
}

// Saves the game's PlayerPrefs before a test and puts them back afterwards, so tests never
// change the player's coins, bests, upgrades or settings.
public class PrefsSnapshot
{
    private static readonly string[] IntKeys = { "coins", "best_kills", "meta_MaxHealth", "meta_Damage", "meta_MoveSpeed", "meta_PickupRange" };
    private static readonly string[] FloatKeys = { "best_time", "music_volume", "sfx_volume" };
    private static readonly string[] StringKeys = { "character" };

    private readonly Dictionary<string, object> m_Saved = new Dictionary<string, object>();

    public void Capture()
    {
        m_Saved.Clear();
        foreach (var key in IntKeys) if (PlayerPrefs.HasKey(key)) m_Saved[key] = PlayerPrefs.GetInt(key);
        foreach (var key in FloatKeys) if (PlayerPrefs.HasKey(key)) m_Saved[key] = PlayerPrefs.GetFloat(key);
        foreach (var key in StringKeys) if (PlayerPrefs.HasKey(key)) m_Saved[key] = PlayerPrefs.GetString(key);
    }

    // Fresh-player state: no coins, upgrades or chosen character
    public static void ClearAll()
    {
        foreach (var key in IntKeys) PlayerPrefs.DeleteKey(key);
        foreach (var key in FloatKeys) PlayerPrefs.DeleteKey(key);
        foreach (var key in StringKeys) PlayerPrefs.DeleteKey(key);
    }

    public void Restore()
    {
        ClearAll();
        foreach (var pair in m_Saved)
        {
            if (pair.Value is int i) PlayerPrefs.SetInt(pair.Key, i);
            else if (pair.Value is float f) PlayerPrefs.SetFloat(pair.Key, f);
            else PlayerPrefs.SetString(pair.Key, (string)pair.Value);
        }
        PlayerPrefs.Save();
    }
}
