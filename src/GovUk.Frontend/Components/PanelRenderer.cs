using System.Text;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class PanelRenderer : IComponentRenderer
{
    public string Name => "panel";

    public string Render(ParamBag parameters)
    {
        var headingLevel = parameters.Truthy("headingLevel") ? parameters.Print("headingLevel") : "1";
        var classes = parameters.Print("classes") ?? "";
        var isInterruption = classes.Contains("govuk-panel--interruption", StringComparison.Ordinal);
        var defaultModifier = isInterruption ? null : "govuk-panel--confirmation";

        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-panel");
        if (defaultModifier is not null)
        {
            builder.Append(' ').Append(defaultModifier);
        }

        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"').Append(parameters.Attributes()).Append('>');
        builder.Append("\n  <h")
            .Append(headingLevel)
            .Append(" class=\"govuk-panel__title\">\n    ");

        if (parameters.Truthy("titleHtml"))
        {
            builder.Append(parameters.Print("titleHtml"));
        }
        else
        {
            builder.Append(Nj.Escape(parameters.Print("titleText")));
        }

        builder.Append("\n  </h").Append(headingLevel).Append('>');

        if (parameters.Truthy("html") || parameters.Truthy("text"))
        {
            builder.Append("\n  <div class=\"govuk-panel__body\">");
            if (parameters.Truthy("html"))
            {
                builder.Append('\n')
                    .Append(Nj.Indent((parameters.Print("html") ?? "").Trim(), 4, first: true));
            }
            else
            {
                builder.Append("\n    ").Append(Nj.Escape(parameters.Print("text")));
            }

            builder.Append("\n  </div>");
        }

        if (isInterruption && parameters.Truthy("actions"))
        {
            var actions = parameters.Get("actions");
            builder.Append("\n  <div class=\"govuk-panel__actions");
            if (actions.Truthy("classes"))
            {
                builder.Append(' ').Append(actions.Print("classes"));
            }

            builder.Append('"').Append(actions.Attributes());

            if (actions.Length("items") > 0)
            {
                builder.Append("><div class=\"govuk-button-group\">");
                foreach (var action in actions.Items("items"))
                {
                    builder.Append('\n')
                        .Append(Nj.Indent(RenderPanelAction(action).Trim(), 6, first: true));
                }

                builder.Append("\n    </div></div>");
            }
            else
            {
                builder.Append("></div>");
            }
        }

        builder.Append("\n</div>");
        return builder.ToString();
    }

    private static string RenderPanelAction(ParamBag action)
    {
        if (action.Truthy("href") && action.Print("type") != "button")
        {
            var builder = new StringBuilder();
            builder.Append("<a class=\"govuk-link govuk-link--inverse");
            if (action.Truthy("classes"))
            {
                builder.Append(' ').Append(action.Print("classes"));
            }

            builder.Append("\" href=\"")
                .Append(action.Escaped("href"))
                .Append('"')
                .Append(action.Attributes())
                .Append('>')
                .Append(action.Print("text"))
                .Append("</a>");
            return builder.ToString();
        }

        var buttonClasses = "govuk-button--inverse";
        if (action.Truthy("classes"))
        {
            buttonClasses += " " + action.Print("classes");
        }

        var buttonParameters = new JsonObject
        {
            ["text"] = action.Print("text"),
            ["type"] = action.Truthy("type") ? action.Print("type") : "button",
            ["classes"] = buttonClasses,
        };
        if (action.Truthy("href"))
        {
            buttonParameters["href"] = action.Print("href");
        }

        if (action.Has("attributes"))
        {
            buttonParameters["attributes"] = JsonNode.Parse(action.Get("attributes").RawJson());
        }

        return new ButtonRenderer().Render(
            ParamBag.FromJson(PhaseBannerRenderer.JsonNodeToElement(buttonParameters)));
    }
}
