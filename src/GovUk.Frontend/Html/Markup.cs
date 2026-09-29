using System.Text;

namespace GovUk.Frontend.Html;

internal static class Markup
{
    public static string ClassName(string baseClass, ParamBag parameters)
    {
        if (!parameters.Truthy("classes"))
        {
            return baseClass;
        }

        return baseClass + " " + parameters.Print("classes");
    }

    /// <summary>
    /// <c>html | safe | trim | indent</c> when html is set, otherwise escaped text.
    /// </summary>
    public static string HtmlOrText(ParamBag parameters, int htmlIndent = 2, bool trimHtml = true)
    {
        if (!parameters.Truthy("html"))
        {
            return Nj.Escape(parameters.Print("text"));
        }

        var html = parameters.Print("html") ?? "";
        if (trimHtml)
        {
            html = html.Trim();
        }

        return htmlIndent > 0 ? Nj.Indent(html, htmlIndent) : html;
    }

    public static string RawOrText(ParamBag parameters, string htmlKey, string textKey)
    {
        if (parameters.Truthy(htmlKey))
        {
            return parameters.Print(htmlKey) ?? "";
        }

        return Nj.Escape(parameters.Print(textKey));
    }

    public static void AppendOptionalId(StringBuilder builder, ParamBag parameters)
    {
        if (parameters.Truthy("id"))
        {
            builder.Append(" id=\"").Append(parameters.Escaped("id")).Append('"');
        }
    }

    public static string Block(string tag, string className, ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append('<').Append(tag);
        AppendOptionalId(builder, parameters);
        builder.Append(" class=\"")
            .Append(Nj.Escape(ClassName(className, parameters)))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  ")
            .Append(HtmlOrText(parameters))
            .Append("\n</")
            .Append(tag)
            .Append('>');
        return builder.ToString();
    }

    /// <summary>
    /// <c>govukI18nAttributes</c>. Plural <paramref name="messages"/> wins over a single message.
    /// </summary>
    public static string I18n(string key, string? message, ParamBag? messages = null)
    {
        if (messages is { IsUndefined: false })
        {
            var builder = new StringBuilder();
            foreach (var (rule, text) in messages.Pairs())
            {
                builder.Append(" data-i18n.")
                    .Append(key)
                    .Append('.')
                    .Append(rule)
                    .Append("=\"")
                    .Append(Nj.Escape(text))
                    .Append('"');
            }

            return builder.ToString();
        }

        if (string.IsNullOrEmpty(message))
        {
            return "";
        }

        return $" data-i18n.{key}=\"{Nj.Escape(message)}\"";
    }
}
