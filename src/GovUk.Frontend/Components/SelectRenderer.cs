using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class SelectRenderer : IComponentRenderer
{
    public string Name => "select";

    public string Render(ParamBag parameters)
    {
        var hasError = parameters.Get("errorMessage").IsTruthy;
        var id = FormFieldHelpers.GetId(parameters);
        var describedBy = FormFieldHelpers.InitialDescribedBy(parameters, emptyWhenMissing: true);

        var builder = new StringBuilder();
        FormFieldHelpers.AppendFormGroupOpen(builder, parameters, hasError);
        builder.Append(FormFieldHelpers.RenderLabel(parameters, id)).Append('\n');

        var hint = parameters.Get("hint");
        if (hint.IsTruthy)
        {
            builder.Append(FormFieldHelpers.RenderHint(parameters, id, ref describedBy));
        }

        if (hasError)
        {
            builder.Append(FormFieldHelpers.RenderErrorMessage(parameters, id, ref describedBy));
        }

        builder.Append(FormFieldHelpers.RenderBeforeInput(parameters, indentWidth: 2, indentFirst: false));
        builder.Append(RenderSelectElement(parameters, id, describedBy, hasError));
        builder.Append(FormFieldHelpers.RenderAfterInput(parameters, indentWidth: 2, indentFirst: false));
        builder.Append("</div>");
        return builder.ToString();
    }

    private static string RenderSelectElement(
        ParamBag parameters,
        string id,
        string describedBy,
        bool hasError)
    {
        var classes = "govuk-select";
        if (parameters.Truthy("classes"))
        {
            classes += " " + parameters.Print("classes");
        }

        if (hasError)
        {
            classes += " govuk-select--error";
        }

        var builder = new StringBuilder();
        builder.Append("  <select class=\"")
            .Append(Nj.Escape(classes))
            .Append("\" id=\"")
            .Append(Nj.Escape(id))
            .Append("\" name=\"")
            .Append(Nj.Escape(parameters.Print("name")))
            .Append('"');

        if (parameters.IsExactlyTrue("disabled"))
        {
            builder.Append(" disabled");
        }

        if (!string.IsNullOrEmpty(describedBy))
        {
            builder.Append(" aria-describedby=\"")
                .Append(Nj.Escape(describedBy))
                .Append('"');
        }

        builder.Append(parameters.Attributes());
        builder.Append(">\n");

        foreach (var item in parameters.Items("items"))
        {
            if (!item.IsTruthy)
            {
                continue;
            }

            builder.Append(RenderOption(item, parameters));
        }

        builder.Append("  </select>\n");
        return builder.ToString();
    }

    private static string RenderOption(ParamBag item, ParamBag parameters)
    {
        var effectiveValue = item.Has("value") ? item.Print("value") : item.Print("text");
        var computedSelected = false;
        if (parameters.Truthy("value"))
        {
            computedSelected = effectiveValue == parameters.Print("value") && !item.IsExactlyFalse("selected");
        }

        var selected = !item.Has("selected") || !item.Truthy("selected")
            ? computedSelected
            : true;

        var builder = new StringBuilder();
        builder.Append("    <option");
        if (item.Has("value"))
        {
            builder.Append(" value=\"")
                .Append(Nj.Escape(item.Print("value")))
                .Append('"');
        }

        if (selected)
        {
            builder.Append(" selected");
        }

        if (item.IsExactlyTrue("disabled"))
        {
            builder.Append(" disabled");
        }

        builder.Append(item.Attributes());
        builder.Append('>');
        builder.Append(Nj.Escape(item.Print("text")));
        builder.Append("</option>\n");
        return builder.ToString();
    }
}
