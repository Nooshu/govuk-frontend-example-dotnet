using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class ExitThisPageRenderer : IComponentRenderer
{
    private const string DefaultHtml =
        "<span class=\"govuk-visually-hidden\">Emergency</span> Exit this page";

    public string Name => "exit-this-page";

    public string Render(ParamBag parameters)
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("<div");
        if (parameters.Has("id") && parameters.Print("id") is not null)
        {
            builder.Append(" id=\"").Append(parameters.Escaped("id")).Append('"');
        }

        builder.Append(" class=\"govuk-exit-this-page");
        if (parameters.Has("classes") && parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append("\" data-module=\"govuk-exit-this-page\"")
            .Append(parameters.Attributes());
        AppendI18n(builder, parameters, "activatedText", "activated");
        AppendI18n(builder, parameters, "timedOutText", "timed-out");
        AppendI18n(builder, parameters, "pressTwoMoreTimesText", "press-two-more-times");
        AppendI18n(builder, parameters, "pressOneMoreTimeText", "press-one-more-time");
        builder.Append(">\n  ");

        var buttonOptions = new JsonObject
        {
            ["classes"] = "govuk-button--warning govuk-exit-this-page__button govuk-js-exit-this-page-button",
            ["href"] = parameters.Default("redirectUrl", "https://www.bbc.co.uk/weather", whenFalsy: true),
            ["attributes"] = new JsonObject { ["rel"] = "nofollow noreferrer" },
        };

        if (parameters.Truthy("html") || parameters.Truthy("text"))
        {
            if (parameters.Truthy("html"))
            {
                buttonOptions["html"] = parameters.Print("html");
            }

            if (parameters.Truthy("text"))
            {
                buttonOptions["text"] = parameters.Print("text");
            }
        }
        else
        {
            buttonOptions["html"] = DefaultHtml;
        }

        var buttonHtml = new ButtonRenderer().Render(
            ParamBag.FromJson(PhaseBannerRenderer.JsonNodeToElement(buttonOptions)));
        builder.Append(Nj.Indent(buttonHtml.Trim(), 2))
            .Append("\n</div>");
        return builder.ToString();
    }

    private static void AppendI18n(
        System.Text.StringBuilder builder,
        ParamBag parameters,
        string key,
        string attribute)
    {
        if (!parameters.Truthy(key))
        {
            return;
        }

        builder.Append(" data-i18n.")
            .Append(attribute)
            .Append("=\"")
            .Append(Nj.Escape(parameters.Print(key)))
            .Append('"');
    }
}
