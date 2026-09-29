using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class SummaryListRenderer : IComponentRenderer
{
    public string Name => "summary-list";

    public string Render(ParamBag parameters)
    {
        var anyRowHasActions = false;
        foreach (var row in parameters.Items("rows"))
        {
            if (row.IsTruthy && row.Get("actions").Length("items") > 0)
            {
                anyRowHasActions = true;
                break;
            }
        }

        var cardTitle = parameters.Get("card").Get("title");
        var summaryList = RenderSummaryList(parameters, anyRowHasActions, cardTitle);

        if (!parameters.Truthy("card"))
        {
            return summaryList;
        }

        return RenderSummaryCard(parameters.Get("card"), summaryList);
    }

    private static string RenderSummaryList(ParamBag parameters, bool anyRowHasActions, ParamBag cardTitle)
    {
        var builder = new StringBuilder();
        builder.Append("<dl class=\"govuk-summary-list");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"').Append(parameters.Attributes()).Append('>');

        foreach (var row in parameters.Items("rows"))
        {
            if (!row.IsTruthy)
            {
                continue;
            }

            builder.Append("\n  <div class=\"govuk-summary-list__row");
            if (anyRowHasActions && row.Get("actions").Length("items") == 0)
            {
                builder.Append(" govuk-summary-list__row--no-actions");
            }

            if (row.Truthy("classes"))
            {
                builder.Append(' ').Append(row.Print("classes"));
            }

            builder.Append("\">\n    <dt class=\"govuk-summary-list__key");
            var key = row.Get("key");
            if (key.Truthy("classes"))
            {
                builder.Append(' ').Append(key.Print("classes"));
            }

            builder.Append("\">\n      ");
            builder.Append(KeyValueContent(key));
            builder.Append("\n    </dt>\n    <dd class=\"govuk-summary-list__value");
            var value = row.Get("value");
            if (value.Truthy("classes"))
            {
                builder.Append(' ').Append(value.Print("classes"));
            }

            builder.Append("\">\n      ");
            builder.Append(KeyValueContent(value));
            builder.Append("\n    </dd>");

            var actions = row.Get("actions");
            if (actions.Length("items") > 0)
            {
                builder.Append("\n    <dd class=\"govuk-summary-list__actions");
                if (actions.Truthy("classes"))
                {
                    builder.Append(' ').Append(actions.Print("classes"));
                }

                builder.Append("\">");
                if (actions.Length("items") == 1)
                {
                    builder.Append("\n      ")
                        .Append(Nj.Trim(Nj.Indent(ActionLink(actions.Items("items")[0], cardTitle), 6, first: true)));
                }
                else
                {
                    builder.Append("\n      <ul class=\"govuk-summary-list__actions-list\">");
                    foreach (var action in actions.Items("items"))
                    {
                        builder.Append("\n        <li class=\"govuk-summary-list__actions-list-item\">")
                            .Append("\n          ")
                            .Append(Nj.Indent(ActionLink(action, cardTitle).Trim(), 8))
                            .Append("\n        </li>");
                    }

                    builder.Append("\n      </ul>");
                }

                builder.Append("\n    </dd>");
            }

            builder.Append("\n  </div>");
        }

        builder.Append("\n</dl>");
        return builder.ToString();
    }

    private static string KeyValueContent(ParamBag item)
    {
        if (item.Truthy("html"))
        {
            return Nj.Indent((item.Print("html") ?? "").Trim(), 6);
        }

        return item.Print("text") ?? "";
    }

    private static string ActionLink(ParamBag action, ParamBag cardTitle)
    {
        var builder = new StringBuilder();
        builder.Append("<a class=\"govuk-link");
        if (action.Truthy("classes"))
        {
            builder.Append(' ').Append(action.Print("classes"));
        }

        builder.Append("\" href=\"")
            .Append(action.Escaped("href"))
            .Append('"')
            .Append(action.Attributes())
            .Append('>');
        if (action.Truthy("html"))
        {
            builder.Append(Nj.Indent(action.Print("html") ?? "", 4));
        }
        else
        {
            builder.Append(action.Print("text"));
        }

        if (action.Truthy("visuallyHiddenText") || cardTitle.IsTruthy)
        {
            builder.Append("<span class=\"govuk-visually-hidden\">");
            if (action.Truthy("visuallyHiddenText"))
            {
                builder.Append(' ').Append(action.Print("visuallyHiddenText"));
            }

            if (cardTitle.IsTruthy)
            {
                builder.Append(" (");
                if (cardTitle.Truthy("html"))
                {
                    builder.Append(Nj.Indent(cardTitle.Print("html") ?? "", 6));
                }
                else
                {
                    builder.Append(cardTitle.Print("text"));
                }

                builder.Append(')');
            }

            builder.Append("</span>");
        }

        builder.Append("</a>");
        return builder.ToString();
    }

    private static string RenderSummaryCard(ParamBag card, string summaryList)
    {
        var headingLevel = card.Get("title").Truthy("headingLevel")
            ? card.Get("title").Print("headingLevel")
            : "2";
        var builder = new StringBuilder();
        builder.Append("<div class=\"govuk-summary-card");
        if (card.Truthy("classes"))
        {
            builder.Append(' ').Append(card.Print("classes"));
        }

        builder.Append('"').Append(card.Attributes()).Append('>');
        builder.Append("\n  <div class=\"govuk-summary-card__title-wrapper\">");

        var title = card.Get("title");
        if (title.IsTruthy)
        {
            builder.Append("\n    <h")
                .Append(headingLevel)
                .Append(" class=\"govuk-summary-card__title");
            if (title.Truthy("classes"))
            {
                builder.Append(' ').Append(title.Print("classes"));
            }

            builder.Append("\">\n      ");
            if (title.Truthy("html"))
            {
                builder.Append(Nj.Indent((title.Print("html") ?? "").Trim(), 6));
            }
            else
            {
                builder.Append(title.Print("text"));
            }

            builder.Append("\n    </h").Append(headingLevel).Append('>');
        }

        var actions = card.Get("actions");
        if (actions.Length("items") > 0)
        {
            if (actions.Length("items") == 1)
            {
                builder.Append("\n    <div class=\"govuk-summary-card__actions");
                if (actions.Truthy("classes"))
                {
                    builder.Append(' ').Append(actions.Print("classes"));
                }

                builder.Append("\">\n      ")
                    .Append(Nj.Indent(ActionLink(actions.Items("items")[0], title).Trim(), 4))
                    .Append("\n    </div>");
            }
            else
            {
                builder.Append("\n    <ul class=\"govuk-summary-card__actions");
                if (actions.Truthy("classes"))
                {
                    builder.Append(' ').Append(actions.Print("classes"));
                }

                builder.Append("\">");
                foreach (var action in actions.Items("items"))
                {
                    builder.Append("\n      <li class=\"govuk-summary-card__action\">")
                        .Append("\n        ")
                        .Append(Nj.Indent(ActionLink(action, title).Trim(), 8))
                        .Append("\n      </li>");
                }

                builder.Append("\n    </ul>");
            }
        }

        builder.Append("\n  </div>\n\n  <div class=\"govuk-summary-card__content\">\n    ")
            .Append(Nj.Indent(summaryList.Trim(), 4))
            .Append("\n  </div>\n</div>");
        return builder.ToString();
    }
}
