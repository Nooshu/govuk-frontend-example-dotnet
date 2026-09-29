namespace GovUk.Frontend.Html;

/// <summary>
/// Nunjucks <c>escape</c> and <c>indent</c>, matching the filters GOV.UK Frontend uses.
/// </summary>
public static class Nj
{
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? "";
        }

        var builder = new System.Text.StringBuilder(value.Length);
        foreach (var character in value)
        {
            switch (character)
            {
                case '&':
                    builder.Append("&amp;");
                    break;
                case '<':
                    builder.Append("&lt;");
                    break;
                case '>':
                    builder.Append("&gt;");
                    break;
                case '"':
                    builder.Append("&quot;");
                    break;
                case '\'':
                    builder.Append("&#39;");
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Nunjucks <c>indent</c>. The first line is indented only when
    /// <paramref name="first"/> is true. An empty string stays empty.
    /// </summary>
    public static string Indent(string? value, int width, bool first = false)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var pad = new string(' ', width);
        var lines = value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            if (index == 0 && !first)
            {
                continue;
            }

            lines[index] = pad + lines[index];
        }

        return string.Join('\n', lines);
    }

    public static string Trim(string? value) => value?.Trim() ?? "";
}
