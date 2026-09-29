using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class ServiceNavigationRenderer : IComponentRenderer
{
    public string Name => "service-navigation";

    public string Render(ParamBag parameters)
    {
        var menuButtonText = parameters.Default("menuButtonText", "Menu", whenFalsy: true);
        var navigationId = parameters.Default("navigationId", "navigation", whenFalsy: true);
        var slots = parameters.Get("slots");
        var (endSlotHtml, endSlotInline) = ParseEndSlot(slots);
        var navigationItems = parameters.Items("navigation").Where(item => item.IsTruthy).ToList();

        var collapseNavigationOnMobile = parameters.Has("collapseNavigationOnMobile")
            ? parameters.IsExactlyTrue("collapseNavigationOnMobile")
            : navigationItems.Count > 1;

        var commonAttributes = new StringBuilder();
        commonAttributes.Append("class=\"govuk-service-navigation");
        if (parameters.Truthy("classes"))
        {
            commonAttributes.Append(' ').Append(parameters.Print("classes"));
        }

        commonAttributes.Append("\"\ndata-module=\"govuk-service-navigation\"")
            .Append(parameters.Attributes())
            .Append('\n');

        var inner = RenderInnerContent(
            parameters,
            slots,
            navigationItems,
            menuButtonText,
            navigationId,
            endSlotHtml,
            endSlotInline,
            collapseNavigationOnMobile);

        var useSection = parameters.Truthy("serviceName")
            || slots.Truthy("start")
            || !string.IsNullOrEmpty(endSlotHtml);
        if (useSection)
        {
            var ariaLabel = parameters.Default("ariaLabel", "Service information");
            return $"<section aria-label=\"{Nj.Escape(ariaLabel)}\" {commonAttributes}>\n{inner}  </section>";
        }

        return $"<div {commonAttributes}>\n{inner}  </div>";
    }

    private static string RenderInnerContent(
        ParamBag parameters,
        ParamBag slots,
        IReadOnlyList<ParamBag> navigationItems,
        string menuButtonText,
        string navigationId,
        string endSlotHtml,
        bool endSlotInline,
        bool collapseNavigationOnMobile)
    {
        var builder = new StringBuilder();
        builder.Append("      <div class=\"govuk-width-container");
        if (endSlotInline)
        {
            builder.Append(" govuk-service-navigation__inlining-container");
        }

        builder.Append("\">\n\n");

        if (slots.Truthy("start"))
        {
            builder.Append("    ").Append(slots.Print("start"));
            builder.Append("<div class=\"govuk-service-navigation__container\">\n");
        }
        else
        {
            builder.Append("    <div class=\"govuk-service-navigation__container\">\n");
        }

        if (parameters.Truthy("serviceName"))
        {
            builder.Append("      \n        <span class=\"govuk-service-navigation__service-name\">\n");
            if (parameters.Truthy("serviceUrl"))
            {
                builder.Append("            <a href=\"")
                    .Append(parameters.Escaped("serviceUrl"))
                    .Append("\" class=\"govuk-service-navigation__link\">\n              ")
                    .Append(Nj.Escape(parameters.Print("serviceName")))
                    .Append("\n            </a>\n");
            }
            else
            {
                builder.Append("            <span class=\"govuk-service-navigation__text\">")
                    .Append(Nj.Escape(parameters.Print("serviceName")))
                    .Append("</span>\n");
            }

            builder.Append("        </span>\n\n");
        }

        var showNav = navigationItems.Count > 0
            || slots.Truthy("navigationStart")
            || slots.Truthy("navigationEnd");
        if (parameters.Truthy("serviceName"))
        {
            builder.Append("      \n");
        }
        else
        {
            builder.Append("      \n\n      \n");
        }

        if (showNav)
        {
            builder.Append("        <nav aria-label=\"")
                .Append(Nj.Escape(parameters.Default("navigationLabel", menuButtonText, whenFalsy: true)))
                .Append("\" class=\"govuk-service-navigation__wrapper");
            if (parameters.Truthy("navigationClasses"))
            {
                builder.Append(' ').Append(parameters.Print("navigationClasses"));
            }

            builder.Append("\">\n");

            if (collapseNavigationOnMobile)
            {
                builder.Append("          <button type=\"button\" class=\"govuk-service-navigation__toggle govuk-js-service-navigation-toggle\" aria-controls=\"")
                    .Append(Nj.Escape(navigationId))
                    .Append('"');
                if (parameters.Truthy("menuButtonLabel")
                    && parameters.Print("menuButtonLabel") != menuButtonText)
                {
                    builder.Append(" aria-label=\"")
                        .Append(Nj.Escape(parameters.Print("menuButtonLabel")))
                        .Append('"');
                }

                builder.Append(" hidden aria-hidden=\"true\">\n            ")
                    .Append(Nj.Escape(menuButtonText))
                    .Append("\n          </button>\n\n");
            }
            else
            {
                builder.Append('\n');
            }

            builder.Append("          <ul class=\"govuk-service-navigation__list\" id=\"")
                .Append(Nj.Escape(navigationId))
                .Append("\" >\n\n");

            if (slots.Truthy("navigationStart"))
            {
                builder.Append("            ").Append(slots.Print("navigationStart")).Append('\n');
            }
            else if (navigationItems.Count > 0)
            {
                builder.Append("            \n");
            }

            foreach (var item in navigationItems)
            {
                builder.Append(RenderNavigationItem(item));
            }

            if (slots.Truthy("navigationEnd"))
            {
                builder.Append("            ").Append(slots.Print("navigationEnd")).Append("</ul>\n        </nav>\n");
            }
            else
            {
                builder.Append("            </ul>\n        </nav>\n");
            }
        }

        builder.Append("    </div>\n\n");

        if (!string.IsNullOrEmpty(endSlotHtml))
        {
            builder.Append("    ").Append(endSlotHtml).Append("</div>\n\n");
        }
        else
        {
            builder.Append("    </div>\n\n");
        }
        return builder.ToString();
    }

    private static string RenderNavigationItem(ParamBag item)
    {
        var inner = RenderLinkInner(item);
        var builder = new StringBuilder();
        builder.Append("              \n              <li class=\"govuk-service-navigation__item");
        if (item.Truthy("active") || item.Truthy("current"))
        {
            builder.Append(" govuk-service-navigation__item--active");
        }

        builder.Append("\">\n");

        if (item.Truthy("href"))
        {
            builder.Append("                  <a class=\"govuk-service-navigation__link\" href=\"")
                .Append(item.Escaped("href"))
                .Append('"');
            if (item.Truthy("active") || item.Truthy("current"))
            {
                builder.Append(" aria-current=\"")
                    .Append(item.Truthy("current") ? "page" : "true")
                    .Append('"');
            }

            builder.Append(item.Attributes())
                .Append(">\n" + new string(' ', 36) + "\n")
                .Append(inner)
                .Append("\n                  </a>\n");
        }
        else if (item.Truthy("html") || item.Truthy("text"))
        {
            builder.Append("                  <span class=\"govuk-service-navigation__text\"");
            if (item.Truthy("active") || item.Truthy("current"))
            {
                builder.Append(" aria-current=\"")
                    .Append(item.Truthy("current") ? "page" : "true")
                    .Append('"');
            }

            builder.Append(">\n" + new string(' ', 36) + "\n")
                .Append(inner)
                .Append("\n                  </span>\n");
        }

        builder.Append("              </li>\n\n");
        return builder.ToString();
    }

    private static (string Html, bool Inline) ParseEndSlot(ParamBag slots)
    {
        if (!slots.Has("end"))
        {
            return ("", false);
        }

        var end = slots.Get("end");
        if (end.Has("html"))
        {
            return (end.Print("html") ?? "", end.Print("align") == "inline");
        }

        return (end.Scalar() ?? "", false);
    }

    private static string RenderLinkInner(ParamBag item)
    {
        var body = item.Truthy("html") ? item.Print("html") ?? "" : Nj.Escape(item.Print("text"));
        if (item.Truthy("active") || item.Truthy("current"))
        {
            return $"                  <strong class=\"govuk-service-navigation__active-fallback\">{body}</strong>\n";
        }

        return body;
    }
}
