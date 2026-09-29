using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class NotificationBannerRenderer : IComponentRenderer
{
    public string Name => "notification-banner";

    public string Render(ParamBag parameters)
    {
        var successBanner = parameters.Print("type") == "success";
        string? typeClass = successBanner ? "govuk-notification-banner--" + parameters.Print("type") : null;

        string role;
        if (parameters.Truthy("role"))
        {
            role = parameters.Print("role")!;
        }
        else if (successBanner)
        {
            role = "alert";
        }
        else
        {
            role = "region";
        }

        string title;
        if (parameters.Truthy("titleHtml"))
        {
            title = parameters.Print("titleHtml")!;
        }
        else if (parameters.Truthy("titleText"))
        {
            title = Nj.Escape(parameters.Print("titleText"));
        }
        else if (successBanner)
        {
            title = "Success";
        }
        else
        {
            title = "Important";
        }

        var titleId = parameters.Default("titleId", "govuk-notification-banner-title", whenFalsy: true);
        var titleHeadingLevel = parameters.Default("titleHeadingLevel", "2", whenFalsy: true);

        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-notification-banner");
        if (typeClass is not null)
        {
            builder.Append(' ').Append(typeClass);
        }

        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append("\" role=\"")
            .Append(Nj.Escape(role))
            .Append("\" aria-labelledby=\"")
            .Append(Nj.Escape(titleId))
            .Append("\" data-module=\"govuk-notification-banner\"");

        if (parameters.Defined("disableAutoFocus"))
        {
            builder.Append(" data-disable-auto-focus=\"")
                .Append(parameters.Print("disableAutoFocus"))
                .Append('"');
        }

        builder.Append(parameters.Attributes()).Append('>');
        builder.Append("\n  <div class=\"govuk-notification-banner__header\">")
            .Append("\n    <h")
            .Append(titleHeadingLevel)
            .Append(" class=\"govuk-notification-banner__title\" id=\"")
            .Append(Nj.Escape(titleId))
            .Append("\">\n      ")
            .Append(title)
            .Append("\n    </h")
            .Append(titleHeadingLevel)
            .Append(">\n  </div>\n  <div class=\"govuk-notification-banner__content\">");

        if (parameters.Truthy("html"))
        {
            builder.Append('\n')
                .Append(Nj.Indent((parameters.Print("html") ?? "").Trim(), 4, first: true));
        }
        else if (parameters.Truthy("text"))
        {
            builder.Append("\n    <p class=\"govuk-notification-banner__heading\">\n      ")
                .Append(Nj.Indent(Nj.Escape(parameters.Print("text")!.Trim()), 6))
                .Append("\n    </p>");
        }

        builder.Append("\n  </div>\n</div>");
        return builder.ToString();
    }
}
