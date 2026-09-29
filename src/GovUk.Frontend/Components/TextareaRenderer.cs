using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class TextareaRenderer : IComponentRenderer
{
    public string Name => "textarea";

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
        builder.Append(RenderTextareaElement(parameters, id, describedBy, hasError));
        builder.Append(FormFieldHelpers.RenderAfterInput(parameters, indentWidth: 2, indentFirst: false));
        builder.Append("</div>");
        return builder.ToString();
    }

    private static string RenderTextareaElement(
        ParamBag parameters,
        string id,
        string describedBy,
        bool hasError)
    {
        var classes = "govuk-textarea";
        if (hasError)
        {
            classes += " govuk-textarea--error";
        }

        if (parameters.Truthy("classes"))
        {
            classes += " " + parameters.Print("classes");
        }

        var builder = new StringBuilder();
        builder.Append("  <textarea class=\"")
            .Append(Nj.Escape(classes))
            .Append("\" id=\"")
            .Append(Nj.Escape(id))
            .Append("\" name=\"")
            .Append(Nj.Escape(parameters.Print("name")))
            .Append("\" rows=\"")
            .Append(Nj.Escape(parameters.Default("rows", "5", whenFalsy: true)))
            .Append('"');

        if (parameters.IsExactlyTrue("spellcheck") || parameters.IsExactlyFalse("spellcheck"))
        {
            builder.Append(" spellcheck=\"")
                .Append(parameters.Print("spellcheck"))
                .Append('"');
        }

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

        if (parameters.Truthy("autocomplete"))
        {
            builder.Append(" autocomplete=\"")
                .Append(parameters.Escaped("autocomplete"))
                .Append('"');
        }

        builder.Append(parameters.Attributes());
        builder.Append('>');
        if (parameters.Has("value") && parameters.Print("value") is not null)
        {
            builder.Append(parameters.Print("value"));
        }

        builder.Append("</textarea>\n");
        return builder.ToString();
    }
}
