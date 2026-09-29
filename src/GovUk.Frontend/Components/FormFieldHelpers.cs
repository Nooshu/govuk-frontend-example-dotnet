using System.Reflection;
using System.Text;
using System.Text.Json;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

internal static class FormFieldHelpers
{
    private static readonly FieldInfo ElementField =
        typeof(ParamBag).GetField("_element", BindingFlags.NonPublic | BindingFlags.Instance)!;

    internal static JsonElement Element(ParamBag bag) => (JsonElement)ElementField.GetValue(bag)!;

    internal static string GetId(ParamBag parameters) =>
        parameters.Truthy("id") ? parameters.Print("id")! : parameters.Print("name")!;

    internal static ParamBag WithProperty(ParamBag bag, string name, string value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value));
        return MergeProperties(bag, new Dictionary<string, JsonElement>(StringComparer.Ordinal)
        {
            [name] = document.RootElement.Clone(),
        });
    }

    internal static ParamBag MergeProperties(
        ParamBag bag,
        IReadOnlyDictionary<string, JsonElement> properties)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var (name, value) in properties)
            {
                writer.WritePropertyName(name);
                value.WriteTo(writer);
            }

            if (!bag.IsUndefined && Element(bag).ValueKind == JsonValueKind.Object)
            {
                foreach (var property in Element(bag).EnumerateObject())
                {
                    if (!properties.ContainsKey(property.Name))
                    {
                        property.WriteTo(writer);
                    }
                }
            }

            writer.WriteEndObject();
        }

        using var document = JsonDocument.Parse(stream.ToArray());
        return ParamBag.FromJson(document.RootElement);
    }

    internal static string FormGroupClass(ParamBag parameters, bool hasError)
    {
        var classes = "govuk-form-group";
        if (hasError)
        {
            classes += " govuk-form-group--error";
        }

        var formGroup = parameters.Get("formGroup");
        if (formGroup.Truthy("classes"))
        {
            classes += " " + formGroup.Print("classes");
        }

        return classes;
    }

    internal static void AppendFormGroupOpen(StringBuilder builder, ParamBag parameters, bool hasError)
    {
        builder.Append("<div class=\"")
            .Append(Nj.Escape(FormGroupClass(parameters, hasError)))
            .Append('"');
        var formGroup = parameters.Get("formGroup");
        if (!formGroup.IsUndefined)
        {
            builder.Append(formGroup.Attributes("attributes"));
        }

        builder.Append(">\n");
    }

    internal static string RenderLabel(ParamBag parameters, string id)
    {
        var label = parameters.Get("label");
        if (label.IsUndefined)
        {
            return "";
        }

        return Nj.Indent(
            Nj.Trim(new LabelRenderer().Render(WithProperty(label, "for", id))),
            2,
            first: true);
    }

    internal static string RenderHint(ParamBag parameters, string id, ref string describedBy)
    {
        var hint = parameters.Get("hint");
        if (hint.IsUndefined || !hint.IsTruthy)
        {
            return "";
        }

        var hintId = id + "-hint";
        describedBy = AppendDescribedBy(describedBy, hintId);
        var hintBag = MergeProperties(hint, new Dictionary<string, JsonElement>(StringComparer.Ordinal)
        {
            ["id"] = JsonDocument.Parse(JsonSerializer.Serialize(hintId)).RootElement,
        });
        return Nj.Indent(Nj.Trim(new HintRenderer().Render(hintBag)), 2, first: true) + "\n";
    }

    internal static string RenderErrorMessage(ParamBag parameters, string id, ref string describedBy)
    {
        var errorMessage = parameters.Get("errorMessage");
        if (errorMessage.IsUndefined || !errorMessage.IsTruthy)
        {
            return "";
        }

        var errorId = id + "-error";
        describedBy = AppendDescribedBy(describedBy, errorId);
        var errorBag = MergeProperties(errorMessage, new Dictionary<string, JsonElement>(StringComparer.Ordinal)
        {
            ["id"] = JsonDocument.Parse(JsonSerializer.Serialize(errorId)).RootElement,
        });
        return Nj.Indent(Nj.Trim(new ErrorMessageRenderer().Render(errorBag)), 2, first: true) + "\n";
    }

    internal static string AppendDescribedBy(string describedBy, string id) =>
        string.IsNullOrEmpty(describedBy) ? id : describedBy + " " + id;

    internal static string InitialDescribedBy(ParamBag parameters, bool emptyWhenMissing)
    {
        if (!parameters.Truthy("describedBy"))
        {
            return emptyWhenMissing ? "" : "";
        }

        return parameters.Print("describedBy") ?? "";
    }

    internal static string RenderBeforeInput(ParamBag parameters, int indentWidth, bool indentFirst)
    {
        var formGroup = parameters.Get("formGroup");
        var beforeInput = formGroup.Get("beforeInput");
        if (beforeInput.IsUndefined || !beforeInput.IsTruthy)
        {
            return "";
        }

        return RenderFormGroupSlot(beforeInput, indentWidth, indentFirst) + "\n";
    }

    internal static string RenderAfterInput(ParamBag parameters, int indentWidth, bool indentFirst)
    {
        var formGroup = parameters.Get("formGroup");
        var afterInput = formGroup.Get("afterInput");
        if (afterInput.IsUndefined || !afterInput.IsTruthy)
        {
            return "";
        }

        return RenderFormGroupSlot(afterInput, indentWidth, indentFirst) + "\n";
    }

    internal static string RenderFormGroupSlot(ParamBag slot, int indentWidth, bool indentFirst)
    {
        if (slot.Truthy("html"))
        {
            var rendered = Nj.Indent(Nj.Trim(slot.Print("html")), indentWidth, first: indentFirst);
            if (!indentFirst && indentWidth > 0)
            {
                rendered = new string(' ', indentWidth) + rendered;
            }

            return rendered;
        }

        return Nj.Escape(slot.Print("text"));
    }

    internal static bool HasAffix(ParamBag affix) =>
        !affix.IsUndefined
        && affix.IsTruthy
        && (affix.Truthy("text") || affix.Truthy("html"));

    internal static string RenderAffix(ParamBag affix, string type)
    {
        var classes = "govuk-input__" + type;
        if (affix.Truthy("classes"))
        {
            classes += " " + affix.Print("classes");
        }

        var content = affix.Truthy("html")
            ? Nj.Indent(Nj.Trim(affix.Print("html")), 4)
            : Nj.Escape(affix.Print("text"));

        return $"<div class=\"{Nj.Escape(classes)}\" aria-hidden=\"true\"{affix.Attributes()}>{content}</div>";
    }
}
