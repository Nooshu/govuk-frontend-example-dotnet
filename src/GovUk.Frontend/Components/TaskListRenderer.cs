using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class TaskListRenderer : IComponentRenderer
{
    public string Name => "task-list";

    public string Render(ParamBag parameters)
    {
        var idPrefix = parameters.Truthy("idPrefix") ? parameters.Print("idPrefix")! : "task-list";
        var classNames = "govuk-task-list";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var builder = new StringBuilder();
        builder.Append("<ul class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n");

        var index = 0;
        foreach (var item in parameters.Items("items"))
        {
            index++;
            if (!item.IsTruthy)
            {
                builder.Append('\n');
                continue;
            }

            builder.Append(RenderItem(item, idPrefix, index));
        }

        builder.Append("</ul>");
        return builder.ToString();
    }

    private static string RenderItem(ParamBag item, string idPrefix, int index)
    {
        var hintId = $"{idPrefix}-{index}-hint";
        var statusId = $"{idPrefix}-{index}-status";
        var classes = "govuk-task-list__item";
        if (item.Truthy("href"))
        {
            classes += " govuk-task-list__item--with-link";
        }

        if (item.Truthy("classes"))
        {
            classes += " " + item.Print("classes");
        }

        var title = item.Get("title");
        var builder = new StringBuilder();
        builder.Append("  <li class=\"").Append(Nj.Escape(classes)).Append("\">\n    <div class=\"govuk-task-list__name-and-hint\">\n");
        if (item.Truthy("href"))
        {
            var describedBy = item.Truthy("hint") ? $"{hintId} {statusId}" : statusId;
            builder.Append("      <a class=\"govuk-link govuk-task-list__link");
            if (title.Truthy("classes"))
            {
                builder.Append(' ').Append(title.Print("classes"));
            }

            builder.Append("\" href=\"")
                .Append(item.Escaped("href"))
                .Append("\" aria-describedby=\"")
                .Append(Nj.Escape(describedBy))
                .Append("\">\n        ")
                .Append(TitleBody(title))
                .Append("\n      </a>\n");
        }
        else
        {
            builder.Append("      <div");
            if (title.Truthy("classes"))
            {
                builder.Append(" class=\"").Append(Nj.Escape(title.Print("classes"))).Append('"');
            }

            builder.Append(">\n        ")
                .Append(TitleBody(title))
                .Append("\n      </div>\n");
        }

        if (item.Truthy("hint"))
        {
            var hint = item.Get("hint");
            builder.Append("      <div id=\"")
                .Append(Nj.Escape(hintId))
                .Append("\" class=\"govuk-task-list__hint\">\n        ")
                .Append(HintBody(hint))
                .Append("\n      </div>\n");
        }

        builder.Append("    </div>\n    <div class=\"govuk-task-list__status");
        var status = item.Get("status");
        if (status.Truthy("classes"))
        {
            builder.Append(' ').Append(status.Print("classes"));
        }

        builder.Append("\" id=\"").Append(Nj.Escape(statusId)).Append("\">\n");
        if (status.Truthy("tag"))
        {
            var tagHtml = ComponentCatalog.Render("tag", status.Get("tag"));
            builder.Append("      ").Append(Nj.Indent(tagHtml.Trim(), 6)).Append('\n');
        }
        else if (status.Truthy("html"))
        {
            builder.Append("      ")
                .Append(Nj.Indent((status.Print("html") ?? "").Trim(), 6))
                .Append('\n');
        }
        else
        {
            builder.Append("      ").Append(Nj.Escape(status.Print("text"))).Append('\n');
        }

        builder.Append("    </div>\n  </li>\n");
        return builder.ToString();
    }

    private static string TitleBody(ParamBag title) =>
        title.Truthy("html")
            ? Nj.Indent((title.Print("html") ?? "").Trim(), 8)
            : Nj.Escape(title.Print("text"));

    private static string HintBody(ParamBag hint) =>
        hint.Truthy("html")
            ? Nj.Indent((hint.Print("html") ?? "").Trim(), 8)
            : Nj.Escape(hint.Print("text"));

}
