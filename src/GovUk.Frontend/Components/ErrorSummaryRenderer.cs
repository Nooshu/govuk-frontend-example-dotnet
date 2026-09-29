using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class ErrorSummaryRenderer : IComponentRenderer
{
    public string Name => "error-summary";

    public string Render(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-error-summary");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"');
        if (parameters.Defined("disableAutoFocus"))
        {
            builder.Append(" data-disable-auto-focus=\"")
                .Append(parameters.Print("disableAutoFocus"))
                .Append('"');
        }

        builder.Append(parameters.Attributes())
            .Append(" data-module=\"govuk-error-summary\">")
            .Append("\n  <div role=\"alert\">")
            .Append("\n    <h2 class=\"govuk-error-summary__title\">");

        if (parameters.Truthy("titleHtml"))
        {
            builder.Append("\n      ")
                .Append(Nj.Indent((parameters.Print("titleHtml") ?? "").Trim(), 6));
        }
        else
        {
            builder.Append("\n      ").Append(Nj.Escape(parameters.Print("titleText")));
        }

        builder.Append("\n    </h2>\n    <div class=\"govuk-error-summary__body\">");

        if (parameters.Truthy("descriptionHtml") || parameters.Truthy("descriptionText"))
        {
            builder.Append("\n      <p>\n        ");
            if (parameters.Truthy("descriptionHtml"))
            {
                builder.Append(Nj.Indent((parameters.Print("descriptionHtml") ?? "").Trim(), 8));
            }
            else
            {
                builder.Append(Nj.Escape(parameters.Print("descriptionText")));
            }

            builder.Append("\n      </p>");
        }

        if (parameters.Length("errorList") > 0)
        {
            builder.Append("\n        <ul class=\"govuk-list govuk-error-summary__list\">");
            foreach (var item in parameters.Items("errorList"))
            {
                builder.Append("\n          <li>");
                if (item.Truthy("href"))
                {
                    builder.Append("\n            <a href=\"")
                        .Append(item.Escaped("href"))
                        .Append('"')
                        .Append(item.Attributes())
                        .Append('>');
                    if (item.Truthy("html"))
                    {
                        builder.Append(Nj.Indent((item.Print("html") ?? "").Trim(), 12));
                    }
                    else
                    {
                        builder.Append(Nj.Escape(item.Print("text")));
                    }

                    builder.Append("</a>");
                }
                else
                {
                    builder.Append("\n            ");
                    if (item.Truthy("html"))
                    {
                        builder.Append(Nj.Indent((item.Print("html") ?? "").Trim(), 10));
                    }
                    else
                    {
                        builder.Append(Nj.Escape(item.Print("text")));
                    }
                }

                builder.Append("\n          </li>");
            }

            builder.Append("\n        </ul>");
        }

        builder.Append("\n    </div>\n  </div>\n</div>");
        return builder.ToString();
    }
}
