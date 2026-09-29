using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class PasswordInputRenderer : IComponentRenderer
{
    public string Name => "password-input";

    public string Render(ParamBag parameters)
    {
        var id = FormFieldHelpers.GetId(parameters);
        var buttonHtml = RenderToggleButton(parameters, id);
        var inputParameters = BuildInputParameters(parameters, id, buttonHtml);
        return Nj.Trim(new InputRenderer().Render(inputParameters));
    }

    private static string BuildFormGroupAttributes(ParamBag parameters)
    {
        var builder = new StringBuilder();
        builder.Append(" data-module=\"govuk-password-input\"");
        builder.Append(Markup.I18n("show-password", parameters.Print("showPasswordText")));
        builder.Append(Markup.I18n("hide-password", parameters.Print("hidePasswordText")));
        builder.Append(Markup.I18n("show-password-aria-label", parameters.Print("showPasswordAriaLabelText")));
        builder.Append(Markup.I18n("hide-password-aria-label", parameters.Print("hidePasswordAriaLabelText")));
        builder.Append(Markup.I18n("password-shown-announcement", parameters.Print("passwordShownAnnouncementText")));
        builder.Append(Markup.I18n("password-hidden-announcement", parameters.Print("passwordHiddenAnnouncementText")));

        var formGroup = parameters.Get("formGroup");
        if (!formGroup.IsUndefined)
        {
            builder.Append(formGroup.Attributes("attributes"));
        }

        return builder.ToString();
    }

    private static string RenderToggleButton(ParamBag parameters, string id)
    {
        var classes = "govuk-button--secondary govuk-password-input__toggle govuk-js-password-input-toggle";
        var button = parameters.Get("button");
        if (button.Truthy("classes"))
        {
            classes += " " + button.Print("classes");
        }

        var buttonParameters = FormFieldHelpers.MergeProperties(
            ParamBag.Undefined,
            new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            {
                ["type"] = JsonDocument.Parse("\"button\"").RootElement,
                ["classes"] = JsonDocument.Parse(JsonSerializer.Serialize(classes)).RootElement,
                ["text"] = JsonDocument.Parse(
                    JsonSerializer.Serialize(parameters.Default("showPasswordText", "Show", whenFalsy: true))).RootElement,
                ["attributes"] = BuildButtonAttributes(id, parameters),
            });

        return new ButtonRenderer().Render(buttonParameters);
    }

    private static JsonElement BuildButtonAttributes(string id, ParamBag parameters)
    {
        var ariaLabel = parameters.Default("showPasswordAriaLabelText", "Show password", whenFalsy: true);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("aria-controls", id);
            writer.WriteString("aria-label", ariaLabel);
            writer.WriteStartObject("hidden");
            writer.WriteBoolean("value", true);
            writer.WriteBoolean("optional", true);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }

    private static ParamBag BuildInputParameters(ParamBag parameters, string id, string buttonHtml)
    {
        var root = JsonNode.Parse(FormFieldHelpers.Element(parameters).GetRawText())!.AsObject();

        var formGroupClasses = "govuk-password-input";
        if (parameters.Get("formGroup").Truthy("classes"))
        {
            formGroupClasses += " " + parameters.Get("formGroup").Print("classes");
        }

        var inputClasses = "govuk-password-input__input govuk-js-password-input-input";
        if (parameters.Truthy("classes"))
        {
            inputClasses += " " + parameters.Print("classes");
        }

        var formGroup = new JsonObject
        {
            ["classes"] = formGroupClasses,
            ["attributes"] = BuildFormGroupAttributes(parameters),
            ["afterInput"] = new JsonObject { ["html"] = buttonHtml },
        };

        if (parameters.Get("formGroup").Truthy("beforeInput"))
        {
            formGroup["beforeInput"] = JsonNode.Parse(
                FormFieldHelpers.Element(parameters.Get("formGroup").Get("beforeInput")).GetRawText());
        }

        root["formGroup"] = formGroup;
        root["inputWrapper"] = new JsonObject { ["classes"] = "govuk-password-input__wrapper" };
        root["classes"] = inputClasses;
        root["id"] = id;
        root["type"] = "password";
        root["spellcheck"] = false;
        root["autocapitalize"] = "none";
        root["autocomplete"] = parameters.Truthy("autocomplete")
            ? parameters.Print("autocomplete")
            : "current-password";

        using var document = JsonDocument.Parse(root.ToJsonString());
        return ParamBag.FromJson(document.RootElement);
    }
}
