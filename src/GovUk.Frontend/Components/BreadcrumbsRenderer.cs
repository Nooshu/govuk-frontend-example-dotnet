using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class BreadcrumbsRenderer : IComponentRenderer
{
    public string Name => "breadcrumbs";

    public string Render(ParamBag parameters)
    {
        var classNames = "govuk-breadcrumbs";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        if (parameters.Truthy("collapseOnMobile"))
        {
            classNames += " govuk-breadcrumbs--collapse-on-mobile";
        }

        var label = parameters.Default("labelText", "Breadcrumb");
        var builder = new StringBuilder();
        builder.Append("<nav class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(" aria-label=\"")
            .Append(Nj.Escape(label))
            .Append("\">\n  <ol class=\"govuk-breadcrumbs__list\">\n");

        foreach (var item in parameters.Items("items"))
        {
            if (item.Truthy("href"))
            {
                builder.Append("    <li class=\"govuk-breadcrumbs__list-item\">\n      <a class=\"govuk-breadcrumbs__link\" href=\"")
                    .Append(item.Escaped("href"))
                    .Append('"')
                    .Append(item.Attributes())
                    .Append('>')
                    .Append(ItemBody(item))
                    .Append("</a>\n    </li>\n");
            }
            else
            {
                builder.Append("    <li class=\"govuk-breadcrumbs__list-item\" aria-current=\"page\">")
                    .Append(ItemBody(item))
                    .Append("</li>\n");
            }
        }

        builder.Append("  </ol>\n</nav>");
        return builder.ToString();
    }

    private static string ItemBody(ParamBag item) =>
        item.Truthy("html") ? item.Print("html") ?? "" : Nj.Escape(item.Print("text"));
}
