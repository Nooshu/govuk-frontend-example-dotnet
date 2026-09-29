using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class InputRenderer : IComponentRenderer
{
    public string Name => "input";

    public string Render(ParamBag parameters)
    {
        var hasError = parameters.Get("errorMessage").IsTruthy;
        var id = FormFieldHelpers.GetId(parameters);
        var describedBy = parameters.Truthy("describedBy") ? parameters.Print("describedBy") ?? "" : "";

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

        var hasPrefix = FormFieldHelpers.HasAffix(parameters.Get("prefix"));
        var hasSuffix = FormFieldHelpers.HasAffix(parameters.Get("suffix"));
        var formGroup = parameters.Get("formGroup");
        var hasBeforeInput = FormFieldHelpers.HasAffix(formGroup.Get("beforeInput"));
        var hasAfterInput = FormFieldHelpers.HasAffix(formGroup.Get("afterInput"));

        if (hasPrefix || hasSuffix || hasBeforeInput || hasAfterInput)
        {
            builder.Append("  <div class=\"")
                .Append(Nj.Escape(InputWrapperClass(parameters)))
                .Append('"')
                .Append(parameters.Get("inputWrapper").Attributes())
                .Append(">\n");
            if (hasBeforeInput)
            {
                builder.Append(FormFieldHelpers.RenderFormGroupSlot(
                    formGroup.Get("beforeInput"),
                    4,
                    indentFirst: true));
                builder.Append('\n');
            }

            if (hasPrefix)
            {
                builder.Append(Nj.Indent(
                    FormFieldHelpers.RenderAffix(parameters.Get("prefix"), "prefix"),
                    4,
                    first: true));
                builder.Append('\n');
            }

            builder.Append(RenderInputElement(parameters, id, describedBy, hasError, indent: 4));

            if (hasSuffix)
            {
                builder.Append('\n');
                builder.Append(Nj.Indent(
                    FormFieldHelpers.RenderAffix(parameters.Get("suffix"), "suffix"),
                    4,
                    first: true));
            }

            if (hasAfterInput)
            {
                builder.Append('\n');
                builder.Append(FormFieldHelpers.RenderFormGroupSlot(
                    formGroup.Get("afterInput"),
                    4,
                    indentFirst: true));
            }

            builder.Append("\n  </div>\n");
        }
        else
        {
            builder.Append(RenderInputElement(parameters, id, describedBy, hasError)).Append('\n');
        }

        builder.Append("</div>");
        return builder.ToString();
    }

    private static string InputWrapperClass(ParamBag parameters)
    {
        var classes = "govuk-input__wrapper";
        var inputWrapper = parameters.Get("inputWrapper");
        if (inputWrapper.Truthy("classes"))
        {
            classes += " " + inputWrapper.Print("classes");
        }

        return classes;
    }

    private static string RenderInputElement(
        ParamBag parameters,
        string id,
        string describedBy,
        bool hasError,
        int indent = 2)
    {
        var pad = new string(' ', indent);
        var classes = "govuk-input";
        if (parameters.Truthy("classes"))
        {
            classes += " " + parameters.Print("classes");
        }

        if (hasError)
        {
            classes += " govuk-input--error";
        }

        var builder = new StringBuilder();
        builder.Append(pad).Append("<input class=\"")
            .Append(Nj.Escape(classes))
            .Append("\" id=\"")
            .Append(Nj.Escape(id))
            .Append("\" name=\"")
            .Append(Nj.Escape(parameters.Print("name")))
            .Append("\" type=\"")
            .Append(Nj.Escape(parameters.Default("type", "text", whenFalsy: true)))
            .Append('"');

        AppendOptionalSpellcheck(builder, parameters);
        AppendOptionalValue(builder, parameters);
        AppendOptionalDisabled(builder, parameters);
        AppendOptionalDescribedBy(builder, describedBy);
        AppendOptionalAttribute(builder, "autocomplete", parameters);
        AppendOptionalAttribute(builder, "autocapitalize", parameters);
        AppendOptionalAttribute(builder, "pattern", parameters);
        AppendOptionalAttribute(builder, "inputmode", parameters);
        builder.Append(parameters.Attributes());
        builder.Append('>');
        return builder.ToString();
    }

    private static void AppendOptionalSpellcheck(StringBuilder builder, ParamBag parameters)
    {
        if (parameters.IsExactlyTrue("spellcheck"))
        {
            builder.Append(" spellcheck=\"true\"");
        }
        else if (parameters.IsExactlyFalse("spellcheck"))
        {
            builder.Append(" spellcheck=\"false\"");
        }
    }

    private static void AppendOptionalValue(StringBuilder builder, ParamBag parameters)
    {
        if (!parameters.Has("value"))
        {
            return;
        }

        if (parameters.Print("value") is null)
        {
            return;
        }

        builder.Append(" value=\"")
            .Append(parameters.Escaped("value"))
            .Append('"');
    }

    private static void AppendOptionalDisabled(StringBuilder builder, ParamBag parameters)
    {
        if (parameters.IsExactlyTrue("disabled"))
        {
            builder.Append(" disabled");
        }
    }

    private static void AppendOptionalDescribedBy(StringBuilder builder, string describedBy)
    {
        if (string.IsNullOrEmpty(describedBy))
        {
            return;
        }

        builder.Append(" aria-describedby=\"")
            .Append(Nj.Escape(describedBy))
            .Append('"');
    }

    private static void AppendOptionalAttribute(StringBuilder builder, string name, ParamBag parameters)
    {
        if (!parameters.Truthy(name))
        {
            return;
        }

        builder.Append(' ')
            .Append(name)
            .Append("=\"")
            .Append(parameters.Escaped(name))
            .Append('"');
    }
}
