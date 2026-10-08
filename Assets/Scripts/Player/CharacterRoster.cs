using System;
using UnityEngine;

// Playable characters live in Resources/Characters so both scenes can list them.
public static class CharacterRoster
{
    private const string Folder = "Characters";

    public static CharacterData[] All()
    {
        CharacterData[] characters = Resources.LoadAll<CharacterData>(Folder);
        Array.Sort(characters, (a, b) => a.sortOrder.CompareTo(b.sortOrder));
        return characters;
    }

    // The saved choice, else `fallback`, else the first character
    public static CharacterData Selected(CharacterData fallback)
    {
        CharacterData[] characters = All();
        string selected = SaveData.SelectedCharacter;
        foreach (var character in characters)
            if (character.name == selected)
                return character;
        if (fallback != null) return fallback;
        return characters.Length > 0 ? characters[0] : null;
    }
}
