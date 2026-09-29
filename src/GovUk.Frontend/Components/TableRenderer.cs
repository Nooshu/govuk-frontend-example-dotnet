using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class TableRenderer : IComponentRenderer
{
    public string Name => "table";

    public string Render(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append("<table class=\"govuk-table");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"').Append(parameters.Attributes()).Append('>');

        if (parameters.Truthy("caption"))
        {
            builder.Append("\n  <caption class=\"govuk-table__caption");
            if (parameters.Truthy("captionClasses"))
            {
                builder.Append(' ').Append(parameters.Print("captionClasses"));
            }

            builder.Append("\">").Append(parameters.Print("caption")).Append("</caption>");
        }

        if (parameters.Length("head") > 0)
        {
            builder.Append("\n  <thead class=\"govuk-table__head\">\n    <tr class=\"govuk-table__row\">");
            foreach (var item in parameters.Items("head"))
            {
                builder.Append(RenderHeadCell(item));
            }

            builder.Append("\n    </tr>\n  </thead>");
        }

        builder.Append("\n  <tbody class=\"govuk-table__body\">");
        foreach (var row in parameters.Items("rows"))
        {
            if (!row.IsTruthy)
            {
                continue;
            }

            builder.Append("\n    <tr class=\"govuk-table__row\">");
            var cellIndex = 0;
            foreach (var cell in row.EnumerateArray())
            {
                cellIndex++;
                builder.Append(RenderBodyCell(cell, parameters.Truthy("firstCellIsHeader") && cellIndex == 1));
            }

            builder.Append("\n    </tr>");
        }

        builder.Append("\n  </tbody>\n</table>");
        return builder.ToString();
    }

    private static string RenderHeadCell(ParamBag item)
    {
        var builder = new StringBuilder();
        builder.Append("\n      <th scope=\"col\" class=\"govuk-table__header");
        if (item.Truthy("format"))
        {
            builder.Append(" govuk-table__header--").Append(item.Print("format"));
        }

        if (item.Truthy("classes"))
        {
            builder.Append(' ').Append(item.Print("classes"));
        }

        builder.Append('"');
        if (item.Truthy("colspan"))
        {
            builder.Append(" colspan=\"").Append(item.Escaped("colspan")).Append('"');
        }

        if (item.Truthy("rowspan"))
        {
            builder.Append(" rowspan=\"").Append(item.Escaped("rowspan")).Append('"');
        }

        builder.Append(item.Attributes()).Append('>');
        builder.Append(CellContent(item));
        builder.Append("</th>");
        return builder.ToString();
    }

    private static string RenderBodyCell(ParamBag cell, bool asHeader)
    {
        var builder = new StringBuilder();
        if (asHeader)
        {
            builder.Append("\n      <th scope=\"row\" class=\"govuk-table__header");
            if (cell.Truthy("classes"))
            {
                builder.Append(' ').Append(cell.Print("classes"));
            }

            builder.Append('"');
            AppendSpanAttributes(builder, cell);
            builder.Append('>');
            builder.Append(CellContent(cell));
            builder.Append("</th>");
        }
        else
        {
            builder.Append("\n      <td class=\"govuk-table__cell");
            if (cell.Truthy("format"))
            {
                builder.Append(" govuk-table__cell--").Append(cell.Print("format"));
            }

            if (cell.Truthy("classes"))
            {
                builder.Append(' ').Append(cell.Print("classes"));
            }

            builder.Append('"');
            AppendSpanAttributes(builder, cell);
            builder.Append('>');
            builder.Append(CellContent(cell));
            builder.Append("</td>");
        }

        return builder.ToString();
    }

    private static void AppendSpanAttributes(StringBuilder builder, ParamBag cell)
    {
        if (cell.Truthy("colspan"))
        {
            builder.Append(" colspan=\"").Append(cell.Escaped("colspan")).Append('"');
        }

        if (cell.Truthy("rowspan"))
        {
            builder.Append(" rowspan=\"").Append(cell.Escaped("rowspan")).Append('"');
        }

        builder.Append(cell.Attributes());
    }

    private static string CellContent(ParamBag cell) =>
        cell.Truthy("html") ? cell.Print("html") ?? "" : Nj.Escape(cell.Print("text"));
}
