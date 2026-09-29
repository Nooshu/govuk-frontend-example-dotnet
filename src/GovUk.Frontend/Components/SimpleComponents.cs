using System.Text;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class ButtonRenderer : IComponentRenderer
{
    private const string StartIcon =
        "\n  <svg class=\"govuk-button__start-icon\" xmlns=\"http://www.w3.org/2000/svg\" width=\"17.5\" height=\"19\" viewBox=\"0 0 33 40\" aria-hidden=\"true\" focusable=\"false\">\n    <path fill=\"currentColor\" d=\"M0 0h13l20 20-20 20H0l20-20z\"/>\n  </svg>";

    public string Name => "button";

    public string Render(ParamBag parameters)
    {
        var classes = "govuk-button";
        if (parameters.Truthy("classes"))
        {
            classes += " " + parameters.Print("classes");
        }

        if (parameters.Truthy("isStartButton"))
        {
            classes += " govuk-button--start";
        }

        var common = $" class=\"{Nj.Escape(classes)}\" data-module=\"govuk-button\""
            + parameters.Attributes();
        if (parameters.Truthy("id"))
        {
            common += $" id=\"{parameters.Escaped("id")}\"";
        }

        var text = ButtonText(parameters);
        var icon = parameters.Truthy("isStartButton") ? StartIcon : "";

        if (parameters.Truthy("href"))
        {
            return $"<a href=\"{parameters.Escaped("href")}\" role=\"button\" draggable=\"false\"{common}>\n  {Nj.Indent(text, 2)}{icon}\n</a>";
        }

        var builder = new StringBuilder();
        builder.Append("<button type=\"").Append(Nj.Escape(parameters.Default("type", "submit"))).Append('"');
        if (parameters.Truthy("value"))
        {
            builder.Append(" value=\"").Append(parameters.Escaped("value")).Append('"');
        }

        if (parameters.Truthy("name"))
        {
            builder.Append(" name=\"").Append(parameters.Escaped("name")).Append('"');
        }

        if (parameters.Truthy("disabled"))
        {
            builder.Append(" disabled aria-disabled=\"true\"");
        }

        if (parameters.Defined("preventDoubleClick"))
        {
            builder.Append(" data-prevent-double-click=\"")
                .Append(parameters.Escaped("preventDoubleClick"))
                .Append('"');
        }

        builder.Append(common)
            .Append(">\n  ")
            .Append(Nj.Indent(text, 2))
            .Append(icon)
            .Append("\n</button>");
        return builder.ToString();
    }

    private static string ButtonText(ParamBag parameters)
    {
        if (!parameters.Truthy("html"))
        {
            return Nj.Escape(parameters.Print("text"));
        }

        var html = (parameters.Print("html") ?? "").Trim();
        return parameters.Truthy("isStartButton") ? $"<span>{html}</span>" : html;
    }
}

public sealed class TagRenderer : IComponentRenderer
{
    public string Name => "tag";

    public string Render(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append("<strong class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-tag", parameters)))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  ")
            .Append(Markup.HtmlOrText(parameters))
            .Append("\n</strong>");
        return builder.ToString();
    }
}

public sealed class HintRenderer : IComponentRenderer
{
    public string Name => "hint";

    public string Render(ParamBag parameters) => Markup.Block("div", "govuk-hint", parameters);
}

public sealed class InsetTextRenderer : IComponentRenderer
{
    public string Name => "inset-text";

    public string Render(ParamBag parameters) => Markup.Block("div", "govuk-inset-text", parameters);
}

public sealed class ErrorMessageRenderer : IComponentRenderer
{
    public string Name => "error-message";

    public string Render(ParamBag parameters)
    {
        var hidden = parameters.Has("visuallyHiddenText")
            ? parameters.Print("visuallyHiddenText") ?? ""
            : "Error";
        var showHidden = !parameters.Has("visuallyHiddenText") || parameters.Truthy("visuallyHiddenText");
        var body = Markup.HtmlOrText(parameters);
        var builder = new StringBuilder();
        builder.Append("<p");
        Markup.AppendOptionalId(builder, parameters);
        builder.Append(" class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-error-message", parameters)))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  ");
        if (showHidden)
        {
            builder.Append("<span class=\"govuk-visually-hidden\">")
                .Append(Nj.Escape(hidden))
                .Append(":</span> ");
        }

        builder.Append(body).Append("\n</p>");
        return builder.ToString();
    }
}

public sealed class LabelRenderer : IComponentRenderer
{
    public string Name => "label";

    public string Render(ParamBag parameters)
    {
        if (!parameters.Truthy("html") && !parameters.Truthy("text"))
        {
            return "";
        }

        var builder = new StringBuilder();
        builder.Append("<label class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-label", parameters)))
            .Append('"')
            .Append(parameters.Attributes());
        if (parameters.Truthy("for"))
        {
            builder.Append(" for=\"").Append(parameters.Escaped("for")).Append('"');
        }

        builder.Append(">\n  ")
            .Append(Markup.HtmlOrText(parameters))
            .Append("\n</label>");

        if (!parameters.Truthy("isPageHeading"))
        {
            return builder.ToString();
        }

        return "<h1 class=\"govuk-label-wrapper\">\n  "
            + Nj.Indent(builder.ToString().Trim(), 2)
            + "\n</h1>";
    }
}

public sealed class WarningTextRenderer : IComponentRenderer
{
    public string Name => "warning-text";

    public string Render(ParamBag parameters)
    {
        var icon = parameters.Default("iconFallbackText", "Warning", whenFalsy: true);
        var body = Markup.RawOrText(parameters, "html", "text");
        var builder = new StringBuilder();
        builder.Append("<div class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-warning-text", parameters)))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  <span class=\"govuk-warning-text__icon\" aria-hidden=\"true\">!</span>\n  <strong class=\"govuk-warning-text__text\">\n    <span class=\"govuk-visually-hidden\">")
            .Append(Nj.Escape(icon))
            .Append("</span>\n    ")
            .Append(body)
            .Append("\n  </strong>\n</div>");
        return builder.ToString();
    }
}

public sealed class DetailsRenderer : IComponentRenderer
{
    public string Name => "details";

    public string Render(ParamBag parameters)
    {
        var summary = parameters.Truthy("summaryHtml")
            ? Nj.Indent((parameters.Print("summaryHtml") ?? "").Trim(), 6)
            : Nj.Escape(parameters.Print("summaryText"));
        var body = Markup.RawOrText(parameters, "html", "text");
        var builder = new StringBuilder();
        builder.Append("<details");
        Markup.AppendOptionalId(builder, parameters);
        builder.Append(" class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-details", parameters)))
            .Append('"')
            .Append(parameters.Attributes());
        if (parameters.Truthy("open"))
        {
            builder.Append(" open");
        }

        builder.Append(">\n  <summary class=\"govuk-details__summary\">\n    <span class=\"govuk-details__summary-text\">\n      ")
            .Append(summary)
            .Append("\n    </span>\n  </summary>\n  <div class=\"govuk-details__text\">\n    ")
            .Append(body)
            .Append("\n  </div>\n</details>");
        return builder.ToString();
    }
}

public sealed class PhaseBannerRenderer : IComponentRenderer
{
    public string Name => "phase-banner";

    public string Render(ParamBag parameters)
    {
        var tag = parameters.Get("tag");
        var tagClasses = "govuk-phase-banner__content__tag";
        if (tag.Truthy("classes"))
        {
            tagClasses += " " + tag.Print("classes");
        }

        var tagParameters = new JsonObject
        {
            ["classes"] = tagClasses,
        };
        if (tag.Has("html"))
        {
            tagParameters["html"] = tag.Print("html");
        }

        if (tag.Has("text"))
        {
            tagParameters["text"] = tag.Print("text");
        }

        var tagHtml = ComponentCatalog.Render("tag", ParamBag.FromJson(JsonNodeToElement(tagParameters)));
        var text = Markup.HtmlOrText(parameters, htmlIndent: 6);
        var builder = new StringBuilder();
        builder.Append("<div class=\"")
            .Append(Nj.Escape(Markup.ClassName("govuk-phase-banner govuk-width-container", parameters)))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  <p class=\"govuk-phase-banner__content\">\n    ")
            .Append(Nj.Indent(tagHtml.Trim(), 4))
            .Append("\n    <span class=\"govuk-phase-banner__text\">\n      ")
            .Append(text)
            .Append("\n    </span>\n  </p>\n</div>");
        return builder.ToString();
    }

    internal static System.Text.Json.JsonElement JsonNodeToElement(JsonNode node)
    {
        using var document = System.Text.Json.JsonDocument.Parse(node.ToJsonString());
        return document.RootElement.Clone();
    }
}

public sealed class SkipLinkRenderer : IComponentRenderer
{
    public string Name => "skip-link";

    public string Render(ParamBag parameters)
    {
        var href = parameters.Default("href", "#content", whenFalsy: true);
        var body = Markup.RawOrText(parameters, "html", "text");
        return $"<a href=\"{Nj.Escape(href)}\" class=\"{Nj.Escape(Markup.ClassName("govuk-skip-link", parameters))}\"{parameters.Attributes()} data-module=\"govuk-skip-link\">{body}</a>";
    }
}

public sealed class BackLinkRenderer : IComponentRenderer
{
    public string Name => "back-link";

    public string Render(ParamBag parameters)
    {
        var href = parameters.Default("href", "#", whenFalsy: true);
        var body = parameters.Truthy("html")
            ? parameters.Print("html") ?? ""
            : Nj.Escape(parameters.Default("text", "Back", whenFalsy: true));
        return $"<a href=\"{Nj.Escape(href)}\" class=\"{Nj.Escape(Markup.ClassName("govuk-back-link", parameters))}\"{parameters.Attributes()}>{body}</a>";
    }
}
