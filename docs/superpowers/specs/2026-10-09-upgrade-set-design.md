# Level-up upgrade set

## Goal
Make the upgrade types the code already supports show up in the level-up selection.

## Assets
Upgrades in `Assets/ScriptableObjects/UpgradeData/`, added to `UpgradeManager.m_AvailableUpgrades` in `GameScene.unity`:

| Asset | Type | Value | Description |
|---|---|---|---|
| Speed | Speed | 0.5 | Move Speed + 0.5 |
| FireRate | FireRate | 0.15 | Fire Rate + 15% |
| Heal | Heal | 30 | Heal 30 HP |
| ProjectileSpeed | ProjectileSpeed | 0.25 | Bullet Speed + 25% |
| Shotgun | AddWeapon | 0 | New weapon: Shotgun (3-way spread) |

Weapon `Assets/ScriptableObjects/GunData/Shotgun.asset`: Spread fire (3 bullets, 30° apart), Bullet prefab,
damage 20, fire rate 1.5 s, range 15, projectile speed 12, same shoot sound as the starting gun.

## Behaviour
- `UpgradeManager.GetUpgradeOptions` only picks from available upgrades:
  - Heal: only when the player is below max HP (`PlayerManager.IsFullHealth`).
  - AddWeapon: only when that weapon is not owned yet (`WeaponSystem.HasWeapon`), so each weapon can be picked once.
  - Fewer than 3 available upgrades means fewer cards are shown.
- `WeaponSystem.ApplyUpgrade` applies weapon stat upgrades to every owned weapon, not only the starting gun.

## Out of scope
MaxAmmo upgrade (ammo is not used by gameplay), rarity, icons.

## Verification
- Scripts compile; every new asset GUID resolves and no GUID is duplicated.
- Play-test: level up several times; all options appear; Heal never offered at full HP;
  Shotgun disappears after being picked; FireRate after Shotgun speeds up both weapons.
