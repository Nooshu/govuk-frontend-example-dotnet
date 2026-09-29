using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class CharacterCountRenderer : IComponentRenderer
{
    public string Name => "character-count";

    public string Render(ParamBag parameters)
    {
        var id = parameters.Truthy("id") ? parameters.Print("id")! : parameters.Print("name")!;
        var hasNoLimit = !parameters.Truthy("maxwords") && !parameters.Truthy("maxlength");
        var textareaDescriptionLength = parameters.Truthy("maxwords")
            ? parameters.Print("maxwords")
            : parameters.Print("maxlength");
        var textareaDescriptionText = parameters.Default(
            "textareaDescriptionText",
            "You can enter up to %{count} " + (parameters.Truthy("maxwords") ? "words" : "characters"),
            whenFalsy: true);
        var textareaDescriptionTextNoLimit = hasNoLimit
            ? null
            : textareaDescriptionText.Replace("%{count}", textareaDescriptionLength ?? "", StringComparison.Ordinal);

        var countMessageHtml = BuildCountMessageHtml(
            parameters,
            id,
            textareaDescriptionTextNoLimit);
        var attributesHtml = BuildAttributesHtml(parameters, hasNoLimit);
        var formGroup = parameters.Get("formGroup");
        foreach (var (name, value) in formGroup.Get("attributes").Pairs())
        {
            attributesHtml += " " + Nj.Escape(name) + "=\"" + Nj.Escape(value) + "\"";
        }

        var formGroupClasses = "govuk-character-count";
        if (formGroup.Truthy("classes"))
        {
            formGroupClasses += " " + formGroup.Print("classes");
        }

        var textareaClasses = "govuk-js-character-count";
        if (parameters.Truthy("classes"))
        {
            textareaClasses += " " + parameters.Print("classes");
        }

        var textareaParams = new JsonObject
        {
            ["id"] = id,
            ["name"] = parameters.Print("name"),
            ["describedBy"] = id + "-info",
            ["rows"] = parameters.Default("rows", "5", whenFalsy: true),
            ["formGroup"] = new JsonObject
            {
                ["classes"] = formGroupClasses,
                ["attributes"] = attributesHtml,
                ["beforeInput"] = CopyNode(formGroup.Get("beforeInput")),
                ["afterInput"] = new JsonObject
                {
                    ["html"] = countMessageHtml,
                },
            },
            ["classes"] = textareaClasses,
            ["label"] = CopyNode(parameters.Get("label")),
            ["attributes"] = CopyNode(parameters.Get("attributes")),
        };
        if (parameters.Has("spellcheck"))
        {
            textareaParams["spellcheck"] = parameters.IsExactlyTrue("spellcheck");
        }

        if (parameters.Truthy("value"))
        {
            textareaParams["value"] = parameters.Print("value");
        }

        if (parameters.Truthy("hint"))
        {
            textareaParams["hint"] = CopyNode(parameters.Get("hint"));
        }

        if (parameters.Truthy("errorMessage"))
        {
            textareaParams["errorMessage"] = CopyNode(parameters.Get("errorMessage"));
        }

        var label = parameters.Get("label");
        if (!label.IsUndefined)
        {
            var labelObject = JsonNode.Parse(label.JsonRoot.GetRawText()) as JsonObject ?? new JsonObject();
            labelObject["for"] = id;
            textareaParams["label"] = labelObject;
        }

        return ComponentCatalog.Render("textarea", FormGroupRendering.Merge(textareaParams)).Trim();
    }

    private static string BuildCountMessageHtml(
        ParamBag parameters,
        string id,
        string? textareaDescriptionTextNoLimit)
    {
        var builder = new StringBuilder();
        var countMessage = parameters.Get("countMessage");
        var hintClasses = "govuk-character-count__message";
        if (countMessage.Truthy("classes"))
        {
            hintClasses += " " + countMessage.Print("classes");
        }

        var hintParams = new JsonObject
        {
            ["text"] = textareaDescriptionTextNoLimit ?? "",
            ["id"] = id + "-info",
            ["classes"] = hintClasses,
        };
        builder.Append(new HintRenderer().Render(FormGroupRendering.Merge(hintParams)).Trim());
        var formGroup = parameters.Get("formGroup");
        if (formGroup.Truthy("afterInput"))
        {
            var afterInput = formGroup.Get("afterInput");
            if (afterInput.Truthy("html"))
            {
                builder.Append(afterInput.Print("html"));
            }
            else if (afterInput.Truthy("text"))
            {
                builder.Append(Nj.Escape(afterInput.Print("text")));
            }
        }

        return builder.ToString();
    }

    private static string BuildAttributesHtml(ParamBag parameters, bool hasNoLimit)
    {
        var builder = new StringBuilder();
        using (var document = JsonDocument.Parse(BuildModuleAttributes(parameters).ToJsonString()))
        {
            builder.Append(ParamBag.RenderAttributes(document.RootElement));
        }
        if (hasNoLimit && parameters.Truthy("textareaDescriptionText"))
        {
            builder.Append(Markup.I18n(
                "textarea-description",
                message: null,
                messages: FormGroupRendering.Merge(new JsonObject
                {
                    ["other"] = parameters.Print("textareaDescriptionText"),
                })));
        }

        builder.Append(Markup.I18n("characters-under-limit", message: null, messages: parameters.Get("charactersUnderLimitText")));
        builder.Append(Markup.I18n("characters-at-limit", parameters.Print("charactersAtLimitText")));
        builder.Append(Markup.I18n("characters-over-limit", message: null, messages: parameters.Get("charactersOverLimitText")));
        builder.Append(Markup.I18n("words-under-limit", message: null, messages: parameters.Get("wordsUnderLimitText")));
        builder.Append(Markup.I18n("words-at-limit", parameters.Print("wordsAtLimitText")));
        builder.Append(Markup.I18n("words-over-limit", message: null, messages: parameters.Get("wordsOverLimitText")));
        return builder.ToString();
    }

    private static JsonObject BuildModuleAttributes(ParamBag parameters)
    {
        var attributes = new JsonObject
        {
            ["data-module"] = "govuk-character-count",
        };
        if (parameters.Truthy("maxlength"))
        {
            attributes["data-maxlength"] = new JsonObject
            {
                ["value"] = parameters.Print("maxlength"),
                ["optional"] = true,
            };
        }

        if (parameters.Truthy("threshold"))
        {
            attributes["data-threshold"] = new JsonObject
            {
                ["value"] = parameters.Print("threshold"),
                ["optional"] = true,
            };
        }

        if (parameters.Truthy("maxwords"))
        {
            attributes["data-maxwords"] = new JsonObject
            {
                ["value"] = parameters.Print("maxwords"),
                ["optional"] = true,
            };
        }

        return attributes;
    }

    private static JsonNode? CopyNode(ParamBag bag) =>
        bag.IsUndefined ? null : JsonNode.Parse(bag.JsonRoot.GetRawText());
}
