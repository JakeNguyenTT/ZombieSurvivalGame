# Game Improvements Implementation Plan

> **For agentic workers:** Executed inline (superpowers:executing-plans style) by the author of this plan. Steps use checkbox (`- [ ]`) syntax for tracking. Code lives in the per-task commits; this plan fixes the decisions, files, interfaces and numbers.

**Goal:** Add enemy variety, time-based difficulty with recurring bosses, combat feedback, smarter level-up choices, and between-run progression to ZombieSurvival.

**Architecture:** Gameplay stays in `Assembly-CSharp` (existing MonoBehaviour singletons). Pure, testable rules (difficulty curve, weighted random, meta costs, run rewards) move into a small `ZombieSurvival.Core` asmdef that `Assembly-CSharp` auto-references, so EditMode tests can reference it. No Unity Editor is available while building, so new UI is created at runtime by cloning existing styled UI elements (font, button) instead of editing scenes; data lives in ScriptableObject assets written as YAML and wired into scenes only through serialized lists.

**Tech Stack:** Unity 6000.6, URP 17.6 (Lit `_BaseColor`), uGUI 2.6 + TextMeshPro, Unity Test Framework 1.8, DOTween, StarterAssets ThirdPersonController.

## Global Constraints

- No new prefabs or scene hierarchy edits; scene YAML edits limited to appending to serialized asset lists.
- New enum values are appended at the end (enums serialize as ints).
- New ScriptableObject fields need initializers; existing assets rely on them.
- All runtime UI reuses the scene's existing TMP font (`shlop SDF`) and button styling.
- Every task: scripts compile (offline Roslyn check), then commit. Final: headless `unity test` EditMode run, then push.

---

## File Structure

| File | Responsibility |
|---|---|
| `Assets/Scripts/Core/ZombieSurvival.Core.asmdef` | Pure rules assembly (auto-referenced) |
| `Assets/Scripts/Core/Difficulty.cs` | Spawn interval, batch size, HP/damage multipliers, boss timing as functions of run time |
| `Assets/Scripts/Core/WeightedRandom.cs` | Weighted pick / pick-N-without-replacement |
| `Assets/Scripts/Core/MetaUpgrades.cs` | `MetaStat` enum, max level, cost and bonus per level |
| `Assets/Scripts/Core/RunRewards.cs` | Coins earned for a run |
| `Assets/Scripts/Manager/SaveData.cs` | PlayerPrefs wrapper: coins, bests, meta levels, selected character, volumes |
| `Assets/Scripts/Enemy/EnemyProjectile.cs` | Pooled spitter projectile built from a primitive at runtime |
| `Assets/Scripts/UI/RuntimeUI.cs` | Helpers to build panels/texts/buttons from templates |
| `Assets/Scripts/UI/DamageNumbers.cs` | Pooled floating damage text on the HUD canvas |
| `Assets/Scripts/UI/BossIndicator.cs` | "BOSS INCOMING" banner + off-screen edge marker |
| `Assets/Scripts/UI/MenuMeta.cs` | Menu: coins/bests, Shop, Character, Settings panels |
| `Assets/Tests/EditMode/*` | EditMode tests (rules + asset/scene integrity) |
| Modified | `EnemyData`, `EnemyBehavior`, `EnemySpawner`, `GameManager`, `PlayerManager`, `WeaponSystem`, `UpgradeData`, `UpgradeManager`, `UpgradePanel`, `UIManager`, `CameraController`, `ExperienceGem`, `ExperienceManager`, `AudioManager`, `CharacterData`, `MenuScene`, `DebugManager`, `Projectile` |

---

### Task 1: Core rules assembly + tests

**Files:** Create `Assets/Scripts/Core/{ZombieSurvival.Core.asmdef,Difficulty.cs,WeightedRandom.cs,MetaUpgrades.cs,RunRewards.cs}`, `Assets/Tests/EditMode/{ZombieSurvival.Tests.EditMode.asmdef,CoreRulesTests.cs}`

**Produces:**
- `Difficulty.SpawnInterval(float t) -> float` = `max(0.15, 1 / (1 + t/60))`
- `Difficulty.SpawnBatch(float t) -> int` = `1 + floor(t/120)`
- `Difficulty.HealthMultiplier(float t)` = `1 + 0.25*t/60`; `Difficulty.DamageMultiplier(float t)` = `1 + 0.1*t/60`
- `Difficulty.FirstBossTime = 90f`, `Difficulty.BossInterval = 120f`, `Difficulty.BossLevelAt(float t) -> int` (0 before first boss, then 1,2,…)
- `WeightedRandom.Pick(IList<float> weights, float roll01) -> int` (-1 if total ≤ 0); `WeightedRandom.PickIndices(IList<float> weights, int count, Func<float> roll01) -> List<int>` (distinct, skips weight ≤ 0)
- `enum MetaStat { MaxHealth, Damage, MoveSpeed, PickupRange }`; `MetaUpgrades.MaxLevel = 5`; `MetaUpgrades.Cost(int level)` = `50 * (level + 1)`; `MetaUpgrades.Bonus(MetaStat, int level)` = level × {10 HP, 0.10 dmg mult, 0.3 speed, 0.5 range}; `MetaUpgrades.Label(MetaStat)`
- `RunRewards.Coins(int kills, float seconds, int bosses)` = `kills/2 + (int)(seconds/10) + bosses*25`

- [ ] Write `CoreRulesTests` covering each formula, boundaries (t=0, clamp at 0.15, boss level before/at/after 90 and 210), weighted pick boundaries, PickIndices distinctness/zero-weight skip, cost/bonus at levels 0 and 5.
- [ ] Implement Core; compile offline (scripts + tests against nunit).
- [ ] Commit `Add core rules assembly with EditMode tests`.

### Task 2: Enemy variety

**Files:** Modify `EnemyData.cs`, `EnemyBehavior.cs`, `EnemySpawner.cs`, `GameManager.cs`, `DebugManager.cs`; create `EnemyProjectile.cs`; create assets `ScriptableObjects/EnemyData/{Runner,Tank,Spitter,Exploder}.asset`; append them to `EnemySpawner.m_EnemyTypes` in `GameScene.unity`; set ZombieData weight.

**Decisions:**
- `enum EnemyArchetype { Walker, Runner, Tank, Spitter, Exploder }`; new `EnemyData` fields: `archetype`, `tint = white`, `scale = 1`, `spawnWeight = 1`, `unlockTime = 0`, `expDrops = 1`, `attackRange = 9`, `attackCooldown = 2.5`, `projectileSpeed = 8`, `explodeRadius = 2.5`, `fuseTime = 0.5`.
- Stats (all use the Zombie prefab):

| Asset | Archetype | Speed | HP | Damage | Tint | Scale | Weight | Unlock (s) | Exp |
|---|---|---|---|---|---|---|---|---|---|
| ZombieData | Walker | 2 | 100 | 10 | white | 1 | 10 | 0 | 1 |
| Runner | Runner | 4 | 50 | 6 | (1, .55, .55) | .9 | 4 | 45 | 1 |
| Spitter | Spitter | 1.8 | 70 | 8 | (.55, 1, .5) | 1 | 3 | 60 | 1 |
| Tank | Tank | 1.3 | 400 | 20 | (.5, .7, 1) | 1.6 | 2 | 90 | 3 |
| Exploder | Exploder | 3 | 40 | 30 | (1, .8, .3) | 1 | 3 | 120 | 1 |

- Spitter stops at `attackRange`, fires `EnemyProjectile` (pooled sphere, distance-checked hit on player, 0.5 radius, 4 s lifetime).
- Exploder: within 1.5 of player starts fuse (`fuseTime`), then damages player if within `explodeRadius`, plays death effect, dies (drops exp).
- Spawner: per-prefab pools (`Dictionary<EnemyBehavior, Queue<EnemyBehavior>>`), weighted pick among `unlockTime <= GameTime`, tint applied via `MaterialPropertyBlock` `_BaseColor`.
- `EnemySpawner.ActiveEnemies` / `GetClosestEnemy` unchanged.

- [ ] Implement; compile; commit `Add runner, spitter, tank and exploder enemies`.

### Task 3: Time-based difficulty + recurring bosses + indicator

**Files:** Modify `EnemySpawner.cs`, `EnemyBehavior.cs`, `GameManager.cs`, `DebugManager.cs`, `UIManager.cs`; create `UI/RuntimeUI.cs`, `UI/BossIndicator.cs`.

**Decisions:**
- Spawn loop uses `Difficulty.SpawnInterval/SpawnBatch(GameTime)`; enemy HP/damage scaled by `HealthMultiplier/DamageMultiplier`. Kill-count difficulty removed.
- Boss when `Difficulty.BossLevelAt(GameTime)` increases: base type `m_EnemyTypes[0]`, HP = base × 10 × level × HealthMultiplier, damage × (2 + level) (one-shot-proof), scale 5, drops 10 extra gems. `EnemySpawner.OnBossSpawned(EnemyBehavior)`, `EnemySpawner.BossesKilled`.
- `EnemySpawner.SpawnBossNow()` for `DebugManager`/`GameManager.SkipToBoss`.
- `BossIndicator`: banner "BOSS INCOMING" 2.5 s; red diamond + "BOSS" label clamped to screen edge (60 px margin) while boss off-screen; hidden on-screen or dead. Camera shake on spawn (Task 4 API).
- `RuntimeUI`: `CreatePanel`, `CreateText`, `CloneButton(template, parent, label, onClick)` (replaces `onClick` event to drop persistent listeners).

- [ ] Implement; compile; commit `Time-based difficulty, recurring bosses and boss indicator`.

### Task 4: Game feel

**Files:** Modify `CameraController.cs`, `EnemyBehavior.cs`, `Projectile.cs`, `PlayerManager.cs`, `UIManager.cs`; create `UI/DamageNumbers.cs`.

**Decisions:**
- `CameraController.Instance.Shake(float strength, float duration)`; player hit 0.15/0.15, explosion 0.4/0.3, boss spawn 0.3/0.5.
- `EnemyBehavior.TakeDamage(float amount, Vector3 hitDirection)`: red flash 0.08 s via `MaterialPropertyBlock`, knockback 4 u/s decaying ×10/s, divided by scale² (bosses barely move).
- `DamageNumbers.Show(Vector3 worldPos, float amount)`: pool of 40 TMP texts on HUD canvas, rise 60 px + fade over 0.6 s.

- [ ] Implement; compile; commit `Add damage numbers, hit flash, knockback and screen shake`.

### Task 5: Magnet, upgrade weights, reroll

**Files:** Modify `UpgradeData.cs` (append `UpgradeType.Magnet`, add `weight = 1`), `UpgradeManager.cs`, `PlayerManager.cs`, `ExperienceGem.cs`, `UpgradePanel.cs`, `UIManager.cs`; create `ScriptableObjects/UpgradeData/Magnet.asset` (value 1, "Pickup Range + 1"); set Shotgun weight 0.5; append Magnet to scene list.

**Decisions:** `PlayerManager.PickupRadius` (base 2), `IncreasePickupRadius`; options picked by `WeightedRandom.PickIndices`; 3 rerolls per run (`UpgradeManager.RerollsLeft`, `UpgradeManager.Reroll()`), reroll button cloned from an upgrade item button, hidden at 0.

- [ ] Implement; compile; commit `Add magnet upgrade, weighted options and rerolls`.

### Task 6: Meta progression

**Files:** Create `Manager/SaveData.cs`, `UI/MenuMeta.cs`, `Resources/Characters/Brawler.asset`; move `ScriptableObjects/PlayerData.asset` → `Resources/Characters/Soldier.asset` (GUID kept); modify `CharacterData.cs` (`displayName`, `description`), `PlayerManager.cs`, `WeaponSystem.cs` (`DamageMultiplier`), `GameManager.cs`, `ExperienceManager.cs` (`CurrentLevel`), `UIManager.cs`, `AudioManager.cs`, `MenuScene.cs`.

**Decisions:**
- Run end: coins = `RunRewards.Coins`, saved; best time / best kills saved; game-over panel shows kills, level, bosses, coins (+ "New best!").
- Menu (runtime-built under existing Canvas): coins + bests line; buttons Shop / Character / Settings next to Start, each opens a panel with Close.
  - Shop: one row per `MetaStat`: "Max Health Lv 2/5 (+20)" + "Buy 150" button; disabled when maxed or unaffordable.
  - Character: Soldier (100 HP, speed 5, rifle) / Brawler (150 HP, speed 4.5, Shotgun start); selected saved by asset name.
  - Settings: Music / SFX volume with −/+ (10% steps), persisted, applied live.
- Game start: character from `Resources/Characters` matching saved name (fallback serialized one); applies `moveSpeed` (sprint = speed + 1), max HP + meta HP, meta speed, meta pickup range, meta damage multiplier on all weapons.

- [ ] Implement; compile; commit `Add coins, permanent upgrades, character select and settings`.

### Task 7: Integrity tests, headless verification, push

**Files:** Create `Assets/Tests/EditMode/AssetIntegrityTests.cs` (loads every UpgradeData/EnemyData/CharacterData asset via `AssetDatabase`, checks required references through `SerializedObject`; opens `GameScene` and checks `UpgradeManager.m_AvailableUpgrades` and `EnemySpawner.m_EnemyTypes` contain no missing references).

- [ ] Run `unity test <project> --mode EditMode`; fix failures until green.
- [ ] Commit; merge to `main`; push.
