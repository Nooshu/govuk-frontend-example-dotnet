using System.Text;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class CheckboxesRenderer : IComponentRenderer
{
    public string Name => "checkboxes";

    public string Render(ParamBag parameters)
    {
        var idPrefix = parameters.Truthy("idPrefix") ? parameters.Print("idPrefix")! : parameters.Print("name")!;
        var fieldset = parameters.Get("fieldset");
        var describedBy = parameters.Truthy("describedBy") ? parameters.Print("describedBy")! : "";
        if (fieldset.Truthy("describedBy"))
        {
            describedBy = fieldset.Print("describedBy")!;
        }

        var hasFieldset = parameters.Truthy("fieldset");
        var innerHtml = BuildInnerHtml(parameters, idPrefix, ref describedBy, hasFieldset);
        var trimmedInner = innerHtml.Trim();
        var body = hasFieldset
            ? FieldsetRenderer.RenderFieldset(FieldsetParameters(fieldset, describedBy), trimmedInner)
            : trimmedInner;

        return $"<div class=\"{Nj.Escape(FormGroupRendering.FormGroupClass(parameters))}\"{FormGroupRendering.FormGroupAttributes(parameters)}>\n  {body}\n</div>";
    }

    private static ParamBag FieldsetParameters(ParamBag fieldset, string describedBy)
    {
        var properties = JsonNode.Parse(fieldset.JsonRoot.GetRawText()) as JsonObject ?? new JsonObject();
        properties["describedBy"] = describedBy;
        return FormGroupRendering.Merge(properties);
    }

    private static string BuildInnerHtml(
        ParamBag parameters,
        string idPrefix,
        ref string describedBy,
        bool hasFieldset)
    {
        var builder = new StringBuilder();
        if (parameters.Truthy("hint"))
        {
            var hintId = idPrefix + "-hint";
            describedBy = FormGroupRendering.AppendDescribedBy(describedBy, hintId);
            builder.Append(FormGroupRendering.RenderHintBlock(hintId, parameters.Get("hint"))).Append('\n');
        }

        if (parameters.Truthy("errorMessage"))
        {
            var errorId = idPrefix + "-error";
            describedBy = FormGroupRendering.AppendDescribedBy(describedBy, errorId);
            var errorBlock = FormGroupRendering.RenderErrorBlock(errorId, parameters.Get("errorMessage"));
            if (parameters.Truthy("hint"))
            {
                errorBlock = "  " + errorBlock;
            }

            builder.Append(errorBlock).Append('\n');
        }

        var checkboxesDivPrefix = parameters.Truthy("hint") || parameters.Truthy("errorMessage") ? "  " : "";
        builder.Append(checkboxesDivPrefix).Append("<div class=\"govuk-checkboxes");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"').Append(parameters.Attributes()).Append(" data-module=\"govuk-checkboxes\">\n");
        var formGroup = parameters.Get("formGroup");
        if (formGroup.Truthy("beforeInputs"))
        {
            builder.Append(FormGroupRendering.RenderFormGroupInsert(formGroup.Get("beforeInputs"), 4)).Append('\n');
        }

        var index = 0;
        foreach (var item in parameters.Items("items"))
        {
            index++;
            if (!item.IsTruthy)
            {
                continue;
            }

            builder.Append(RenderItem(parameters, item, idPrefix, index, describedBy, hasFieldset));
        }

        if (formGroup.Truthy("afterInputs"))
        {
            builder.Append(FormGroupRendering.RenderFormGroupInsert(formGroup.Get("afterInputs"), 4)).Append('\n');
        }

        TrimTrailingNewline(builder);
        builder.Append("\n  </div>");
        return builder.ToString();
    }

    private static string RenderItem(
        ParamBag parameters,
        ParamBag item,
        string idPrefix,
        int index,
        string describedBy,
        bool hasFieldset)
    {
        if (item.Truthy("divider"))
        {
            return $"    <div class=\"govuk-checkboxes__divider\">{Nj.Escape(item.Print("divider"))}</div>\n";
        }

        var itemId = FormGroupRendering.ItemId(idPrefix, index, item);
        var itemName = item.Truthy("name") ? item.Print("name")! : parameters.Print("name")!;
        var isChecked = FormGroupRendering.CheckboxIsChecked(parameters, item);
        var conditionalId = "conditional-" + itemId;
        var hint = item.Get("hint");
        var hasHint = hint.Truthy("text") || hint.Truthy("html");
        var itemHintId = hasHint ? itemId + "-item-hint" : "";
        var itemDescribedBy = hasFieldset ? "" : describedBy;
        if (hasHint)
        {
            itemDescribedBy = string.IsNullOrEmpty(itemDescribedBy)
                ? itemHintId
                : itemDescribedBy + " " + itemHintId;
        }

        var builder = new StringBuilder();
        builder.Append("    <div class=\"govuk-checkboxes__item\">\n      <input class=\"govuk-checkboxes__input\" id=\"")
            .Append(Nj.Escape(itemId))
            .Append("\" name=\"")
            .Append(Nj.Escape(itemName))
            .Append("\" type=\"checkbox\" value=\"")
            .Append(Nj.Escape(item.Print("value")))
            .Append('"');
        if (isChecked)
        {
            builder.Append(" checked");
        }

        if (item.Truthy("disabled"))
        {
            builder.Append(" disabled");
        }

        var conditional = item.Get("conditional");
        if (conditional.Truthy("html"))
        {
            builder.Append(" data-aria-controls=\"").Append(Nj.Escape(conditionalId)).Append('"');
        }

        if (item.Truthy("behaviour"))
        {
            builder.Append(" data-behaviour=\"").Append(Nj.Escape(item.Print("behaviour"))).Append('"');
        }

        if (!string.IsNullOrEmpty(itemDescribedBy))
        {
            builder.Append(" aria-describedby=\"").Append(Nj.Escape(itemDescribedBy.Trim())).Append('"');
        }

        builder.Append(item.Attributes()).Append(">\n");
        builder.Append(Nj.Indent(RenderItemLabel(item, itemId), 6, first: true)).Append('\n');
        if (hasHint)
        {
            builder.Append(Nj.Indent(RenderItemHint(item, itemId), 6, first: true)).Append('\n');
        }

        builder.Append("    </div>\n");
        if (conditional.Truthy("html"))
        {
            builder.Append("    <div class=\"govuk-checkboxes__conditional");
            if (!isChecked)
            {
                builder.Append(" govuk-checkboxes__conditional--hidden");
            }

            builder.Append("\" id=\"").Append(Nj.Escape(conditionalId)).Append("\">\n      ");
            builder.Append((conditional.Print("html") ?? "").Trim()).Append("\n    </div>\n");
        }

        return builder.ToString();
    }

    private static string RenderItemLabel(ParamBag item, string itemId)
    {
        var label = item.Get("label");
        var classes = "govuk-checkboxes__label";
        if (label.Truthy("classes"))
        {
            classes += " " + label.Print("classes");
        }

        var properties = new JsonObject
        {
            ["classes"] = classes,
            ["for"] = itemId,
        };
        if (item.Truthy("html"))
        {
            properties["html"] = item.Print("html");
        }

        if (item.Truthy("text"))
        {
            properties["text"] = item.Print("text");
        }

        if (label.Has("attributes"))
        {
            properties["attributes"] = JsonNode.Parse(label.Get("attributes").JsonRoot.GetRawText());
        }

        return new LabelRenderer().Render(FormGroupRendering.Merge(properties)).Trim();
    }

    private static string RenderItemHint(ParamBag item, string itemId)
    {
        var hint = item.Get("hint");
        var classes = "govuk-checkboxes__hint";
        if (hint.Truthy("classes"))
        {
            classes += " " + hint.Print("classes");
        }

        var properties = new JsonObject
        {
            ["id"] = itemId + "-item-hint",
            ["classes"] = classes,
        };
        if (hint.Truthy("html"))
        {
            properties["html"] = hint.Print("html");
        }

        if (hint.Truthy("text"))
        {
            properties["text"] = hint.Print("text");
        }

        if (hint.Has("attributes"))
        {
            properties["attributes"] = JsonNode.Parse(hint.Get("attributes").JsonRoot.GetRawText());
        }

        return new HintRenderer().Render(FormGroupRendering.Merge(properties)).Trim();
    }

    private static void TrimTrailingNewline(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] == '\n')
        {
            builder.Length -= 1;
        }
    }
}
