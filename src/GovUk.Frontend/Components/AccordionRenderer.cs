using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class AccordionRenderer : IComponentRenderer
{
    public string Name => "accordion";

    public string Render(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-accordion");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append("\" data-module=\"govuk-accordion\" id=\"")
            .Append(parameters.Escaped("id"))
            .Append('"');
        builder.Append(Markup.I18n("hide-all-sections", parameters.Print("hideAllSectionsText")));
        builder.Append(Markup.I18n("hide-section", parameters.Print("hideSectionText")));
        builder.Append(Markup.I18n("hide-section-aria-label", parameters.Print("hideSectionAriaLabelText")));
        builder.Append(Markup.I18n("show-all-sections", parameters.Print("showAllSectionsText")));
        builder.Append(Markup.I18n("show-section", parameters.Print("showSectionText")));
        builder.Append(Markup.I18n("show-section-aria-label", parameters.Print("showSectionAriaLabelText")));
        if (parameters.Defined("rememberExpanded"))
        {
            builder.Append(" data-remember-expanded=\"")
                .Append(Nj.Escape(parameters.Print("rememberExpanded")))
                .Append('"');
        }

        builder.Append(parameters.Attributes()).Append('>');

        var headingLevel = parameters.Truthy("headingLevel")
            ? parameters.Print("headingLevel")
            : "2";
        var id = parameters.Print("id") ?? "";
        var index = 0;
        foreach (var item in parameters.Items("items"))
        {
            index++;
            if (!item.IsTruthy)
            {
                continue;
            }

            builder.Append(RenderItem(item, id, index, headingLevel!));
        }

        builder.Append("\n</div>");
        return builder.ToString();
    }

    private static string RenderItem(ParamBag item, string id, int index, string headingLevel)
    {
        var builder = new StringBuilder();
        builder.Append("\n  <div class=\"govuk-accordion__section");
        if (item.Truthy("expanded"))
        {
            builder.Append(" govuk-accordion__section--expanded");
        }

        builder.Append("\">\n    <div class=\"govuk-accordion__section-header\">\n      <h")
            .Append(headingLevel)
            .Append(" class=\"govuk-accordion__section-heading\">\n        <span class=\"govuk-accordion__section-button\" id=\"")
            .Append(Nj.Escape(id))
            .Append("-heading-")
            .Append(index)
            .Append("\">\n          ");
        builder.Append(HeadingContent(item.Get("heading")));
        builder.Append("\n        </span>\n      </h")
            .Append(headingLevel)
            .Append(">");

        var summary = item.Get("summary");
        if (summary.Truthy("html") || summary.Truthy("text"))
        {
            builder.Append("\n      <div class=\"govuk-accordion__section-summary govuk-body\" id=\"")
                .Append(Nj.Escape(id))
                .Append("-summary-")
                .Append(index)
                .Append("\">\n        ");
            if (summary.Truthy("html"))
            {
                builder.Append(Nj.Indent((summary.Print("html") ?? "").Trim(), 8));
            }
            else
            {
                builder.Append(summary.Print("text"));
            }

            builder.Append("\n      </div>");
        }

        builder.Append("\n    </div>\n    <div id=\"")
            .Append(Nj.Escape(id))
            .Append("-content-")
            .Append(index)
            .Append("\" class=\"govuk-accordion__section-content\">");
        builder.Append(ContentBlock(item.Get("content")));
        builder.Append("\n    </div>\n  </div>");
        return builder.ToString();
    }

    private static string HeadingContent(ParamBag heading)
    {
        if (heading.Truthy("html"))
        {
            return Nj.Indent((heading.Print("html") ?? "").Trim(), 8);
        }

        return heading.Print("text") ?? "";
    }

    private static string ContentBlock(ParamBag content)
    {
        if (content.Truthy("html"))
        {
            return "\n      " + Nj.Indent((content.Print("html") ?? "").Trim(), 6);
        }

        if (!content.Truthy("text"))
        {
            return "";
        }

        return "\n      <p class=\"govuk-body\">\n        "
            + Nj.Indent(content.Print("text")!.Trim(), 8)
            + "\n      </p>";
    }
}
