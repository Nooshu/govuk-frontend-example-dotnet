using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class TabsRenderer : IComponentRenderer
{
    public string Name => "tabs";

    public string Render(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append("<div");
        if (parameters.Truthy("id"))
        {
            builder.Append(" id=\"").Append(parameters.Escaped("id")).Append('"');
        }

        builder.Append(" class=\"govuk-tabs");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"')
            .Append(parameters.Attributes())
            .Append(" data-module=\"govuk-tabs\">");

        var title = parameters.Default("title", "Contents");
        builder.Append("\n  <h2 class=\"govuk-tabs__title\">\n    ")
            .Append(Nj.Escape(title))
            .Append("\n  </h2>");

        if (parameters.Length("items") > 0)
        {
            var idPrefix = parameters.Truthy("idPrefix") ? parameters.Print("idPrefix") ?? "" : "";
            builder.Append("\n  <ul class=\"govuk-tabs__list\">");
            var index = 0;
            foreach (var item in parameters.Items("items"))
            {
                index++;
                if (!item.IsTruthy)
                {
                    continue;
                }

                builder.Append(RenderTabListItem(item, idPrefix, index));
            }

            builder.Append("\n  </ul>");

            index = 0;
            foreach (var item in parameters.Items("items"))
            {
                index++;
                if (!item.IsTruthy)
                {
                    continue;
                }

                builder.Append(RenderTabPanel(item, idPrefix, index));
            }
        }

        builder.Append("\n</div>");
        return builder.ToString();
    }

    private static string TabPanelId(ParamBag item, string idPrefix, int index) =>
        item.Truthy("id") ? item.Print("id")! : idPrefix + "-" + index;

    private static string RenderTabListItem(ParamBag item, string idPrefix, int index)
    {
        var tabPanelId = TabPanelId(item, idPrefix, index);
        var selected = index == 1 ? " govuk-tabs__list-item--selected" : "";
        var inner = $"<li class=\"govuk-tabs__list-item{selected}\">\n  <a class=\"govuk-tabs__tab\" href=\"#{Nj.Escape(tabPanelId)}\"{item.Attributes()}>\n    {Nj.Escape(item.Print("label"))}\n  </a>\n</li>";
        return "\n    " + Nj.Trim(Nj.Indent(inner, 4, first: true));
    }

    private static string RenderTabPanel(ParamBag item, string idPrefix, int index)
    {
        var tabPanelId = TabPanelId(item, idPrefix, index);
        var hidden = index > 1 ? " govuk-tabs__panel--hidden" : "";
        var panel = item.Get("panel");
        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-tabs__panel")
            .Append(hidden)
            .Append("\" id=\"")
            .Append(Nj.Escape(tabPanelId))
            .Append('"')
            .Append(panel.Attributes())
            .Append('>');

        if (panel.Truthy("html"))
        {
            builder.Append('\n').Append(Nj.Indent((panel.Print("html") ?? "").Trim(), 2, first: true));
        }
        else if (panel.Truthy("text"))
        {
            builder.Append("\n  <p class=\"govuk-body\">")
                .Append(Nj.Escape(panel.Print("text")))
                .Append("</p>");
        }

        builder.Append("\n</div>");
        return "\n  " + Nj.Trim(Nj.Indent(builder.ToString(), 2, first: true));
    }
}
