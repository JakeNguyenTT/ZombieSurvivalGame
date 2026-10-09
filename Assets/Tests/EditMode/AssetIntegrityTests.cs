using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Checks the data wiring that the compiler can't: scene lists, asset references and missing scripts.
// Game types live in Assembly-CSharp, which test assemblies can't reference, so fields are read
// through SerializedObject by name.
public class AssetIntegrityTests
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string MenuScenePath = "Assets/Scenes/MenuScene.unity";
    private const int AddWeaponType = 1; // UpgradeType.AddWeapon
    private const int EvolveType = 10;   // UpgradeType.Evolve
    private const int LastProjectileFiringType = 2; // FiringType.Automatic

    [Test]
    public void UpgradeManager_OffersValidUpgrades()
    {
        WithScene(GameScenePath, scene =>
        {
            var upgrades = ReadList(FindComponent(scene, "UpgradeManager"), "m_AvailableUpgrades");
            Assert.That(upgrades.Count, Is.GreaterThanOrEqualTo(3), "Level-up needs at least 3 upgrades");
            foreach (var upgrade in upgrades)
            {
                var data = new SerializedObject(upgrade);
                Assert.That(data.FindProperty("description").stringValue, Is.Not.Empty, upgrade.name);
                Assert.That(data.FindProperty("weight").floatValue, Is.GreaterThan(0f), upgrade.name);
                int type = data.FindProperty("type").intValue;
                if (type == AddWeaponType)
                    AssertWeaponValid(data.FindProperty("weaponData").objectReferenceValue, upgrade.name);
                if (type == EvolveType)
                {
                    Object evolved = data.FindProperty("weaponData").objectReferenceValue;
                    AssertWeaponValid(evolved, upgrade.name);
                    Object baseWeapon = new SerializedObject(evolved).FindProperty("evolvesFrom").objectReferenceValue;
                    Assert.IsNotNull(baseWeapon, $"{evolved.name} does not say what it evolves from");
                    Assert.IsTrue(OffersWeapon(upgrades, baseWeapon), $"{baseWeapon.name} has no weapon card, so {evolved.name} can never be reached");
                }
            }
        });
    }

    [Test]
    public void EnemySpawner_HasValidEnemyTypes()
    {
        WithScene(GameScenePath, scene =>
        {
            var types = ReadList(FindComponent(scene, "EnemySpawner"), "m_EnemyTypes");
            Assert.That(types, Is.Not.Empty);
            foreach (var type in types)
            {
                var data = new SerializedObject(type);
                Assert.IsNotNull(data.FindProperty("prefab").objectReferenceValue, $"{type.name} has no prefab");
                Assert.That(data.FindProperty("spawnWeight").floatValue, Is.GreaterThan(0f), type.name);
                Assert.That(data.FindProperty("health").floatValue, Is.GreaterThan(0f), type.name);
            }
            // The first type is the boss base and the fallback, so it must be available from the start
            Assert.That(new SerializedObject(types[0]).FindProperty("unlockTime").floatValue, Is.EqualTo(0f));
        });
    }

    [Test]
    public void Characters_HaveStartingWeapons()
    {
        ScriptableObject[] characters = Resources.LoadAll<ScriptableObject>("Characters");
        Assert.That(characters.Length, Is.GreaterThanOrEqualTo(2));
        var names = new HashSet<string>();
        foreach (var character in characters)
        {
            Assert.IsTrue(names.Add(character.name), $"Duplicate character name {character.name}");
            var data = new SerializedObject(character);
            Assert.That(data.FindProperty("displayName").stringValue, Is.Not.Empty, character.name);
            Assert.That(data.FindProperty("maxHealth").floatValue, Is.GreaterThan(0f), character.name);
            AssertWeaponValid(data.FindProperty("startingWeapon").objectReferenceValue, character.name);
        }
    }

    [Test]
    public void ArenaProps_ReferenceExistingPrefabs()
    {
        ScriptableObject set = Resources.Load<ScriptableObject>("ArenaProps");
        Assert.IsNotNull(set, "Resources/ArenaProps missing");
        var data = new SerializedObject(set);
        foreach (string field in new[] { "solidProps", "decorProps" })
        {
            SerializedProperty list = data.FindProperty(field);
            Assert.That(list.arraySize, Is.GreaterThan(0), field);
            for (int i = 0; i < list.arraySize; i++)
                Assert.IsNotNull(list.GetArrayElementAtIndex(i).objectReferenceValue, $"{field}[{i}] is missing");
        }
    }

    [TestCase(GameScenePath)]
    [TestCase(MenuScenePath)]
    public void Scene_HasNoMissingScripts(string path)
    {
        WithScene(path, scene =>
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject), Is.EqualTo(0),
                        $"Missing script on {t.name}");
        });
    }

    private static void AssertWeaponValid(Object weapon, string owner)
    {
        Assert.IsNotNull(weapon, $"{owner} has no weapon");
        var data = new SerializedObject(weapon);
        int firingType = data.FindProperty("firingType").intValue;
        // Single, Spread and Automatic shoot projectile prefabs; Orbit, Homing and Aura build their own visuals
        if (firingType <= LastProjectileFiringType)
            Assert.IsNotNull(data.FindProperty("projectilePrefab").objectReferenceValue, $"{weapon.name} has no projectile");
        Assert.That(data.FindProperty("fireRate").floatValue, Is.GreaterThan(0f), weapon.name);
        Assert.That(data.FindProperty("damage").floatValue, Is.GreaterThan(0f), weapon.name);
        Assert.That(data.FindProperty("count").intValue, Is.GreaterThan(0), weapon.name);
    }

    // True when some AddWeapon card in the pool hands out `weapon`
    private static bool OffersWeapon(List<Object> upgrades, Object weapon)
    {
        foreach (var upgrade in upgrades)
        {
            var data = new SerializedObject(upgrade);
            if (data.FindProperty("type").intValue == AddWeaponType &&
                data.FindProperty("weaponData").objectReferenceValue == weapon)
                return true;
        }
        return false;
    }

    private static void WithScene(string path, System.Action<Scene> check)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            check(scene);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static MonoBehaviour FindComponent(Scene scene, string typeName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null && behaviour.GetType().Name == typeName)
                    return behaviour;
        Assert.Fail($"{typeName} not found in {scene.path}");
        return null;
    }

    private static List<Object> ReadList(Object owner, string field)
    {
        SerializedProperty list = new SerializedObject(owner).FindProperty(field);
        Assert.IsNotNull(list, $"{field} not found");
        var items = new List<Object>();
        for (int i = 0; i < list.arraySize; i++)
        {
            Object item = list.GetArrayElementAtIndex(i).objectReferenceValue;
            Assert.IsNotNull(item, $"{field}[{i}] is missing");
            items.Add(item);
        }
        return items;
    }
}
