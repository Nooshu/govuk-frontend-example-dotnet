using System.Text;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class CookieBannerRenderer : IComponentRenderer
{
    public string Name => "cookie-banner";

    public string Render(ParamBag parameters)
    {
        var classNames = "govuk-cookie-banner";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var ariaLabel = parameters.Default("ariaLabel", "Cookie banner", whenFalsy: true);
        var builder = new StringBuilder();
        builder.Append("<div class=\"")
            .Append(Nj.Escape(classNames))
            .Append("\" data-nosnippet role=\"region\" aria-label=\"")
            .Append(Nj.Escape(ariaLabel))
            .Append('"');
        if (parameters.Truthy("hidden"))
        {
            builder.Append(" hidden");
        }

        builder.Append(parameters.Attributes()).Append(">\n");

        foreach (var message in parameters.Items("messages"))
        {
            builder.Append(RenderMessage(message));
        }

        builder.Append("</div>");
        return builder.ToString();
    }

    private static string RenderMessage(ParamBag message)
    {
        var classNames = "govuk-cookie-banner__message";
        if (message.Truthy("classes"))
        {
            classNames += " " + message.Print("classes");
        }

        classNames += " govuk-width-container";

        var builder = new StringBuilder();
        builder.Append("  <div class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"');
        if (message.Truthy("role"))
        {
            builder.Append(" role=\"").Append(message.Escaped("role")).Append('"');
        }

        builder.Append(message.Attributes());
        if (message.Truthy("hidden"))
        {
            builder.Append(" hidden");
        }

        builder.Append(">\n\n    <div class=\"govuk-grid-row\">\n      <div class=\"govuk-grid-column-two-thirds\">\n");

        if (message.Truthy("headingHtml") || message.Truthy("headingText"))
        {
            builder.Append("        <h2 class=\"govuk-cookie-banner__heading govuk-heading-m\">\n          ");
            if (message.Truthy("headingHtml"))
            {
                builder.Append(Nj.Indent((message.Print("headingHtml") ?? "").Trim(), 10));
            }
            else
            {
                builder.Append(Nj.Escape(message.Print("headingText")));
            }

            builder.Append("\n        </h2>\n");
        }

        builder.Append("        <div class=\"govuk-cookie-banner__content\">\n");
        if (message.Truthy("html"))
        {
            builder.Append("          ")
                .Append(Nj.Indent((message.Print("html") ?? "").Trim(), 10))
                .Append('\n');
        }
        else if (message.Truthy("text"))
        {
            builder.Append("          <p class=\"govuk-body\">")
                .Append(Nj.Escape(message.Print("text")))
                .Append("</p>\n");
        }

        builder.Append("        </div>\n      </div>\n    </div>\n\n");

        if (message.Has("actions"))
        {
            builder.Append("    <div class=\"govuk-button-group\">\n");
            foreach (var action in message.Items("actions"))
            {
                builder.Append("      ").Append(Nj.Indent(RenderAction(action).Trim(), 6)).Append("\n");
            }

            builder.Append("    </div>\n");
        }

        builder.Append("\n  </div>\n");
        return builder.ToString();
    }

    private static string RenderAction(ParamBag action)
    {
        var isButton = !action.Truthy("href") || action.Print("type") == "button";
        if (isButton)
        {
            var buttonOptions = new JsonObject
            {
                ["text"] = action.Print("text"),
                ["type"] = action.Has("type") ? action.Print("type") : "button",
            };
            CopyIfPresent(action, buttonOptions, "name");
            CopyIfPresent(action, buttonOptions, "value");
            CopyIfPresent(action, buttonOptions, "classes");
            CopyIfPresent(action, buttonOptions, "href");
            if (action.Has("attributes"))
            {
                buttonOptions["attributes"] = JsonNode.Parse(action.Get("attributes").Scalar() ?? "{}");
            }

            return new ButtonRenderer().Render(
                ParamBag.FromJson(PhaseBannerRenderer.JsonNodeToElement(buttonOptions)));
        }

        var linkClass = "govuk-link";
        if (action.Truthy("classes"))
        {
            linkClass += " " + action.Print("classes");
        }

        return $"<a class=\"{Nj.Escape(linkClass)}\" href=\"{action.Escaped("href")}\"{action.Attributes()}>{Nj.Escape(action.Print("text"))}</a>";
    }

    private static void CopyIfPresent(ParamBag source, JsonObject target, string key)
    {
        if (source.Has(key))
        {
            target[key] = source.Print(key);
        }
    }
}
