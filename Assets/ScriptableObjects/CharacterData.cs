using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public WeaponData startingWeapon;
}