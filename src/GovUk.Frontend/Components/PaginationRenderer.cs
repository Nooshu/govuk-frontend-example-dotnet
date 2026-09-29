using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class PaginationRenderer : IComponentRenderer
{
    private const string ArrowPrevious =
        "  <svg class=\"govuk-pagination__icon govuk-pagination__icon--prev\" xmlns=\"http://www.w3.org/2000/svg\" height=\"13\" width=\"15\" aria-hidden=\"true\" focusable=\"false\" viewBox=\"0 0 15 13\">\n    <path d=\"m6.5938-0.0078125-6.7266 6.7266 6.7441 6.4062 1.377-1.449-4.1856-3.9768h12.896v-2h-12.984l4.2931-4.293-1.414-1.414z\"></path>\n  </svg>";

    private const string ArrowNext =
        "  <svg class=\"govuk-pagination__icon govuk-pagination__icon--next\" xmlns=\"http://www.w3.org/2000/svg\" height=\"13\" width=\"15\" aria-hidden=\"true\" focusable=\"false\" viewBox=\"0 0 15 13\">\n    <path d=\"m8.107-0.0078125-1.4136 1.414 4.2926 4.293h-12.986v2h12.896l-4.1855 3.9766 1.377 1.4492 6.7441-6.4062-6.7246-6.7266z\"></path>\n  </svg>";

    public string Name => "pagination";

    public string Render(ParamBag parameters)
    {
        var previous = parameters.Get("previous");
        var next = parameters.Get("next");
        var hasItemList = parameters.Has("items") && parameters.Length("items") > 0;
        var blockLevel = !hasItemList && (previous.Truthy("href") || next.Truthy("href"));

        var classNames = "govuk-pagination";
        if (blockLevel)
        {
            classNames += " govuk-pagination--block";
        }

        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var landmark = parameters.Default("landmarkLabel", "Pagination", whenFalsy: true);
        var builder = new StringBuilder();
        builder.Append("<nav class=\"")
            .Append(Nj.Escape(classNames))
            .Append("\" aria-label=\"")
            .Append(Nj.Escape(landmark))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n");

        if (previous.Truthy("href"))
        {
            builder.Append(RenderArrowLink(previous, "prev", blockLevel));
        }

        if (hasItemList)
        {
            builder.Append("  <ul class=\"govuk-pagination__list\">\n");
            foreach (var item in parameters.Items("items"))
            {
                if (!IncludePageItem(item))
                {
                    continue;
                }

                builder.Append(RenderPageItem(item)).Append('\n');
            }

            builder.Append("  </ul>\n");
        }

        if (next.Truthy("href"))
        {
            builder.Append(RenderArrowLink(next, "next", blockLevel));
        }

        builder.Append("</nav>");
        return builder.ToString();
    }

    private static bool IncludePageItem(ParamBag item) => item.IsTruthy && item.Pairs().Any();

    private static string RenderPageItem(ParamBag item)
    {
        var classes = "govuk-pagination__item";
        if (item.Truthy("current"))
        {
            classes += " govuk-pagination__item--current";
        }

        if (item.Truthy("ellipsis"))
        {
            classes += " govuk-pagination__item--ellipsis";
        }

        var builder = new StringBuilder();
        builder.Append("      <li class=\"").Append(classes).Append("\">\n");
        if (item.Truthy("ellipsis"))
        {
            builder.Append("      &ctdot;\n");
        }
        else
        {
            var label = item.Default("visuallyHiddenText", "Page " + item.Print("number"));
            builder.Append("      <a class=\"govuk-link govuk-pagination__link\" href=\"")
                .Append(item.Escaped("href"))
                .Append("\" aria-label=\"")
                .Append(Nj.Escape(label))
                .Append('"');
            if (item.Truthy("current"))
            {
                builder.Append(" aria-current=\"page\"");
            }

            builder.Append(item.Attributes())
                .Append(">\n        ")
                .Append(Nj.Escape(item.Print("number")))
                .Append("\n      </a>\n");
        }

        builder.Append("    </li>");
        return builder.ToString();
    }

    private static string RenderArrowLink(ParamBag link, string type, bool blockLevel)
    {
        var arrow = Nj.Indent(type == "prev" ? ArrowPrevious : ArrowNext, 4, first: true);
        var builder = new StringBuilder();
        builder.Append("  <div class=\"govuk-pagination__").Append(type).Append("\">\n    ")
            .Append("<a class=\"govuk-link govuk-pagination__link\" href=\"")
            .Append(link.Escaped("href"))
            .Append("\" rel=\"")
            .Append(type == "prev" ? "prev" : "next")
            .Append('"')
            .Append(link.Attributes())
            .Append(">\n");

        if (blockLevel || type == "prev")
        {
            builder.Append(arrow).Append("\n");
        }

        var titleClasses = "govuk-pagination__link-title";
        if (blockLevel && !link.Truthy("labelText"))
        {
            titleClasses += " govuk-pagination__link-title--decorated";
        }

        builder.Append("      <span class=\"").Append(titleClasses).Append("\">\n        ")
            .Append(ArrowLinkTitle(link, type))
            .Append("\n      </span>\n");

        if (link.Truthy("labelText") && blockLevel)
        {
            builder.Append("      <span class=\"govuk-visually-hidden\">:</span>\n      <span class=\"govuk-pagination__link-label\">")
                .Append(Nj.Escape(link.Print("labelText")))
                .Append("</span>\n");
        }

        if (!blockLevel && type == "next")
        {
            builder.Append(arrow).Append("\n");
        }

        builder.Append("    </a>\n  </div>\n");
        return builder.ToString();
    }

    private static string ArrowLinkTitle(ParamBag link, string type)
    {
        if (link.Truthy("html"))
        {
            return Nj.Indent((link.Print("html") ?? "").Trim(), 8);
        }

        if (link.Truthy("text"))
        {
            return Nj.Escape(link.Print("text"));
        }

        return type == "prev"
            ? "Previous<span class=\"govuk-visually-hidden\"> page</span>"
            : "Next<span class=\"govuk-visually-hidden\"> page</span>";
    }
}
