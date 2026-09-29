using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class FieldsetRenderer : IComponentRenderer
{
    public string Name => "fieldset";

    public string Render(ParamBag parameters)
    {
        var html = parameters.Truthy("html") ? parameters.Print("html") : null;
        return RenderFieldset(parameters, html, blankLineBeforeClose: html is not null);
    }

    /// <summary>
    /// Renders a fieldset. When <paramref name="blockIndent"/> is set, applies
    /// Nunjucks <c>indent(n, first: false)</c> to the trimmed output (date-input).
    /// </summary>
    internal static string RenderFieldset(
        ParamBag parameters,
        string? htmlContent,
        int? blockIndent = null,
        bool blankLineBeforeClose = false)
    {
        var builder = new StringBuilder();
        builder.Append("<fieldset class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-fieldset", parameters)))
            .Append('"');
        if (parameters.Truthy("role"))
        {
            builder.Append(" role=\"").Append(parameters.Escaped("role")).Append('"');
        }

        if (parameters.Truthy("describedBy"))
        {
            builder.Append(" aria-describedby=\"").Append(parameters.Escaped("describedBy")).Append('"');
        }

        builder.Append(parameters.Attributes()).Append(">\n");
        AppendLegend(builder, parameters);
        if (!string.IsNullOrEmpty(htmlContent))
        {
            builder.Append(PrependSpacesToFirstLine(htmlContent, 2));
            if (!blankLineBeforeClose)
            {
                builder.Append('\n');
            }
        }

        if (blankLineBeforeClose)
        {
            builder.Append('\n');
        }

        builder.Append("</fieldset>");
        var output = builder.ToString();
        if (blockIndent is null)
        {
            return output;
        }

        return Nj.Indent(output.Trim(), blockIndent.Value, first: false);
    }

    private static void AppendLegend(StringBuilder builder, ParamBag parameters)
    {
        var legend = parameters.Get("legend");
        if (!legend.Truthy("html") && !legend.Truthy("text"))
        {
            return;
        }

        builder.Append("  <legend class=\"govuk-fieldset__legend");
        if (legend.Truthy("classes"))
        {
            builder.Append(' ').Append(legend.Print("classes"));
        }

        builder.Append("\">\n");
        if (legend.IsExactlyTrue("isPageHeading"))
        {
            builder.Append("    <h1 class=\"govuk-fieldset__heading\">\n      ");
            builder.Append(LegendBody(legend, htmlIndent: 6));
            builder.Append("\n    </h1>\n");
        }
        else
        {
            builder.Append("    ");
            builder.Append(LegendBody(legend, htmlIndent: 4));
            builder.Append('\n');
        }

        builder.Append("  </legend>\n");
    }

    private static string PrependSpacesToFirstLine(string value, int spaces)
    {
        var pad = new string(' ', spaces);
        var newline = value.IndexOf('\n');
        return newline < 0 ? pad + value : pad + value[..newline] + value[newline..];
    }

    private static string LegendBody(ParamBag legend, int htmlIndent)
    {
        if (legend.Truthy("html"))
        {
            var html = (legend.Print("html") ?? "").Trim();
            return htmlIndent > 0 ? Nj.Indent(html, htmlIndent) : html;
        }

        return Nj.Escape(legend.Print("text"));
    }
}
