using System.Text;

public static class TextUtil
{
    // "ProjectileSpeed" -> "Projectile Speed", "MaxHP" -> "Max HP"
    public static string Nicify(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var builder = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (i > 0 && char.IsUpper(c))
            {
                char previous = name[i - 1];
                bool wordStart = char.IsLower(previous) || char.IsDigit(previous);
                bool acronymEnd = char.IsUpper(previous) && i + 1 < name.Length && char.IsLower(name[i + 1]);
                if (wordStart || acronymEnd) builder.Append(' ');
            }
            builder.Append(c == '_' ? ' ' : c);
        }
        return builder.ToString();
    }
}
