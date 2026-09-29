using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class FeedbackRenderer : IComponentRenderer
{
    public string Name => "feedback";

    public string Render(ParamBag parameters)
    {
        var headingLevel = parameters.Truthy("headingLevel") ? parameters.Print("headingLevel") : "2";
        var classNames = "govuk-feedback govuk-width-container";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var title = parameters.Truthy("titleHtml")
            ? parameters.Print("titleHtml") ?? ""
            : Nj.Escape(parameters.Print("titleText"));

        var builder = new StringBuilder();
        builder.Append("<div class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  <div class=\"govuk-grid-row\">\n    <div class=\"govuk-grid-column-two-thirds\">\n      <h")
            .Append(headingLevel)
            .Append(" class=\"govuk-feedback__title\">\n        ")
            .Append(title)
            .Append("\n      </h")
            .Append(headingLevel)
            .Append('>');

        if (parameters.Truthy("html") || parameters.Truthy("text"))
        {
            builder.Append("\n        <div class=\"govuk-feedback__body\">\n");
            if (parameters.Truthy("html"))
            {
                builder.Append("            ")
                    .Append(Nj.Indent((parameters.Print("html") ?? "").Trim(), 4))
                    .Append('\n');
            }
            else
            {
                builder.Append("            <p class=\"govuk-body\">\n              ")
                    .Append(Nj.Indent(Nj.Escape(Nj.Trim(parameters.Print("text"))), 6))
                    .Append("\n            </p>\n");
            }

            builder.Append("        </div>");
        }

        builder.Append("\n    </div>\n  </div>\n</div>");
        return builder.ToString();
    }
}
