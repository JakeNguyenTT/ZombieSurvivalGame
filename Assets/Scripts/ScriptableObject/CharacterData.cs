using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string displayName = "Character";
    [TextArea] public string description;
    public int sortOrder;
    public float moveSpeed = 5f;
    public float maxHealth = 100f;
    public WeaponData startingWeapon;
}
