using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class FileUploadRenderer : IComponentRenderer
{
    public string Name => "file-upload";

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

        if (parameters.IsExactlyTrue("javascript"))
        {
            builder.Append(RenderJavascriptWrapperOpen(parameters));
        }

        builder.Append(RenderFileInput(parameters, id, describedBy, hasError));

        if (parameters.IsExactlyTrue("javascript"))
        {
            builder.Append("  </div>\n");
        }

        builder.Append(FormFieldHelpers.RenderAfterInput(parameters, indentWidth: 2, indentFirst: false));
        builder.Append("</div>");
        return builder.ToString();
    }

    private static string RenderJavascriptWrapperOpen(ParamBag parameters)
    {
        var classes = "govuk-file-upload-wrapper";
        if (parameters.Truthy("wrapperClasses"))
        {
            classes += " " + parameters.Print("wrapperClasses");
        }

        var i18n = BuildI18nAttributes(parameters);
        var wrapperAttributes = parameters.Attributes("wrapperAttributes");

        var builder = new StringBuilder();
        builder.Append("  <div\n    class=\"")
            .Append(Nj.Escape(classes))
            .Append("\"\n    data-module=\"govuk-file-upload\"");
        builder.Append(i18n);
        builder.Append(wrapperAttributes);
        builder.Append("\n  >\n");
        return builder.ToString();
    }

    private static string BuildI18nAttributes(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append(Markup.I18n("choose-files-button", parameters.Print("chooseFilesButtonText")));
        builder.Append(Markup.I18n("no-file-chosen", parameters.Print("noFileChosenText")));
        builder.Append(Markup.I18n(
            "multiple-files-chosen",
            message: null,
            messages: parameters.Get("multipleFilesChosenText")));
        builder.Append(Markup.I18n("drop-instruction", parameters.Print("dropInstructionText")));
        builder.Append(Markup.I18n("entered-drop-zone", parameters.Print("enteredDropZoneText")));
        builder.Append(Markup.I18n("left-drop-zone", parameters.Print("leftDropZoneText")));
        return builder.ToString();
    }

    private static string RenderFileInput(
        ParamBag parameters,
        string id,
        string describedBy,
        bool hasError)
    {
        var classes = "govuk-file-upload";
        if (parameters.Truthy("classes"))
        {
            classes += " " + parameters.Print("classes");
        }

        if (hasError)
        {
            classes += " govuk-file-upload--error";
        }

        var builder = new StringBuilder();
        builder.Append("  <input class=\"")
            .Append(Nj.Escape(classes))
            .Append("\" id=\"")
            .Append(Nj.Escape(id))
            .Append("\" name=\"")
            .Append(Nj.Escape(parameters.Print("name")))
            .Append("\" type=\"file\"");

        if (parameters.IsExactlyTrue("disabled"))
        {
            builder.Append(" disabled");
        }

        if (parameters.IsExactlyTrue("multiple"))
        {
            builder.Append(" multiple");
        }

        if (!string.IsNullOrEmpty(describedBy))
        {
            builder.Append(" aria-describedby=\"")
                .Append(Nj.Escape(describedBy))
                .Append('"');
        }

        builder.Append(parameters.Attributes());
        builder.Append(">\n");
        return builder.ToString();
    }
}
