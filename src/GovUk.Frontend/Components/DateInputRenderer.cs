using System.Text;
using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class DateInputRenderer : IComponentRenderer
{
    public string Name => "date-input";

    public string Render(ParamBag parameters)
    {
        var fieldset = parameters.Get("fieldset");
        var describedBy = fieldset.Truthy("describedBy") ? fieldset.Print("describedBy")! : "";
        var defaults = BuildDefaultItems(parameters);
        var innerHtml = BuildInnerHtml(parameters, ref describedBy, defaults);
        var trimmedInner = innerHtml.Trim();
        var body = parameters.Truthy("fieldset")
            ? FieldsetRenderer.RenderFieldset(
                FieldsetParameters(fieldset, describedBy),
                trimmedInner,
                blockIndent: 2)
            : trimmedInner;

        return $"<div class=\"{Nj.Escape(FormGroupRendering.FormGroupClass(parameters))}\"{FormGroupRendering.FormGroupAttributes(parameters)}>\n  {body}\n</div>";
    }

    private static ParamBag FieldsetParameters(ParamBag fieldset, string describedBy)
    {
        var properties = JsonNode.Parse(fieldset.JsonRoot.GetRawText()) as JsonObject ?? new JsonObject();
        properties["describedBy"] = describedBy;
        properties["role"] = "group";
        return FormGroupRendering.Merge(properties);
    }

    private static string BuildInnerHtml(ParamBag parameters, ref string describedBy, DateDefaults defaults)
    {
        var builder = new StringBuilder();
        if (parameters.Truthy("hint"))
        {
            var hintId = parameters.Print("id") + "-hint";
            describedBy = FormGroupRendering.AppendDescribedBy(describedBy, hintId!);
            builder.Append(FormGroupRendering.RenderHintBlock(hintId!, parameters.Get("hint"))).Append('\n');
        }

        if (parameters.Truthy("errorMessage"))
        {
            var errorId = parameters.Print("id") + "-error";
            describedBy = FormGroupRendering.AppendDescribedBy(describedBy, errorId!);
            var errorBlock = FormGroupRendering.RenderErrorBlock(errorId!, parameters.Get("errorMessage"));
            if (parameters.Truthy("hint"))
            {
                errorBlock = "  " + errorBlock;
            }

            builder.Append(errorBlock).Append('\n');
        }

        var dateInputPrefix = parameters.Truthy("hint") || parameters.Truthy("errorMessage") ? "  " : "";
        builder.Append(dateInputPrefix).Append("<div class=\"govuk-date-input");
        if (parameters.Truthy("classes"))
        {
            builder.Append(' ').Append(parameters.Print("classes"));
        }

        builder.Append('"').Append(parameters.Attributes());
        if (parameters.Truthy("id"))
        {
            builder.Append(" id=\"").Append(parameters.Escaped("id")).Append('"');
        }

        builder.Append(">\n");
        var formGroup = parameters.Get("formGroup");
        if (formGroup.Truthy("beforeInputs"))
        {
            builder.Append(FormGroupRendering.RenderFormGroupInsert(formGroup.Get("beforeInputs"), 4)).Append('\n');
        }

        var items = ResolveItems(parameters, defaults);
        var anyItemHasError = items.Any(item => ItemHasError(item));
        foreach (var item in items)
        {
            if (!item.IsTruthy)
            {
                continue;
            }

            builder.Append(RenderDateItem(parameters, item, defaults, anyItemHasError));
        }

        if (formGroup.Truthy("afterInputs"))
        {
            builder.Append(FormGroupRendering.RenderFormGroupInsert(formGroup.Get("afterInputs"), 4)).Append('\n');
        }

        TrimTrailingNewline(builder);
        builder.Append("\n  </div>");
        return builder.ToString();
    }

    private static DateDefaults BuildDefaultItems(ParamBag parameters)
    {
        var values = parameters.Get("values");
        return new DateDefaults(
            MergeItemDefault(parameters.Get("day"), new JsonObject
            {
                ["name"] = "day",
                ["value"] = values.Print("day"),
                ["classes"] = "govuk-input--width-2",
            }),
            MergeItemDefault(parameters.Get("month"), new JsonObject
            {
                ["name"] = "month",
                ["value"] = values.Print("month"),
                ["classes"] = "govuk-input--width-2",
            }),
            MergeItemDefault(parameters.Get("year"), new JsonObject
            {
                ["name"] = "year",
                ["value"] = values.Print("year"),
                ["classes"] = "govuk-input--width-4",
            }));
    }

    private static List<ParamBag> ResolveItems(ParamBag parameters, DateDefaults defaults)
    {
        if (parameters.Length("items") > 0)
        {
            return parameters.Items("items").ToList();
        }

        var items = new List<ParamBag>();
        if (!parameters.IsExactlyFalse("day"))
        {
            items.Add(defaults.Day);
        }

        if (!parameters.IsExactlyFalse("month"))
        {
            items.Add(defaults.Month);
        }

        if (!parameters.IsExactlyFalse("year"))
        {
            items.Add(defaults.Year);
        }

        return items;
    }

    private static ParamBag MergeItemDefault(ParamBag item, JsonObject defaults)
    {
        if (!item.IsUndefined)
        {
            var merged = JsonNode.Parse(item.JsonRoot.GetRawText()) as JsonObject ?? new JsonObject();
            foreach (var (key, value) in defaults)
            {
                merged.TryAdd(key, value?.DeepClone());
            }

            return FormGroupRendering.Merge(merged);
        }

        return FormGroupRendering.Merge(defaults);
    }

    private static bool ItemHasError(ParamBag item) =>
        item.IsExactlyTrue("error")
        || (item.Truthy("classes") && item.Print("classes")!.Contains("govuk-input--error", StringComparison.Ordinal));

    private static string RenderDateItem(
        ParamBag parameters,
        ParamBag item,
        DateDefaults defaults,
        bool anyItemHasError)
    {
        var itemName = item.Print("name");
        var itemValue = item.Has("value") ? item.Print("value") : null;
        var itemWidth = 2;
        ResolveItemIdentity(item, defaults, ref itemName, ref itemValue, ref itemWidth);

        var itemClasses = BuildItemClasses(item, parameters, anyItemHasError, itemWidth);
        var namePrefix = parameters.Truthy("namePrefix") ? parameters.Print("namePrefix") + "-" : "";
        var inputId = item.Truthy("id")
            ? item.Print("id")!
            : parameters.Print("id") + "-" + itemName;
        var labelText = ResolveLabelText(item, itemName!);
        var resolvedValue = ResolveItemValue(item, itemValue, parameters, namePrefix, itemName!);

        var inputParams = BuildInputParams(item, inputId, namePrefix + itemName, resolvedValue, itemClasses, labelText);
        var inputHtml = Nj.Indent(
            ComponentCatalog.Render("input", FormGroupRendering.Merge(inputParams)).Trim(),
            6,
            first: true);
        return "    <div class=\"govuk-date-input__item\">\n" + inputHtml + "\n    </div>\n";
    }

    private static void ResolveItemIdentity(
        ParamBag item,
        DateDefaults defaults,
        ref string? itemName,
        ref string? itemValue,
        ref int itemWidth)
    {
        if (IsSameItem(item, defaults.Day) || NameMatches(itemName, "day", defaults.Day.Print("name")))
        {
            itemName = item.Truthy("name") ? item.Print("name") : "day";
            itemValue ??= item.Has("value") ? item.Print("value") : defaults.Day.Print("value");
            return;
        }

        if (IsSameItem(item, defaults.Month) || NameMatches(itemName, "month", defaults.Month.Print("name")))
        {
            itemName = item.Truthy("name") ? item.Print("name") : "month";
            itemValue ??= item.Has("value") ? item.Print("value") : defaults.Month.Print("value");
            return;
        }

        if (IsSameItem(item, defaults.Year) || NameMatches(itemName, "year", defaults.Year.Print("name")))
        {
            itemName = item.Truthy("name") ? item.Print("name") : "year";
            itemValue ??= item.Has("value") ? item.Print("value") : defaults.Year.Print("value");
            itemWidth = 4;
        }
    }

    private static bool IsSameItem(ParamBag item, ParamBag defaults) =>
        item.JsonRoot.GetRawText() == defaults.JsonRoot.GetRawText();

    private static bool NameMatches(string? itemName, string canonical, string? defaultName) =>
        itemName is not null && (itemName == canonical || itemName == defaultName);

    private static string BuildItemClasses(
        ParamBag item,
        ParamBag parameters,
        bool anyItemHasError,
        int itemWidth)
    {
        var itemClasses = "";
        var classesText = item.Print("classes") ?? "";
        var hasErrorClass = classesText.Contains("govuk-input--error", StringComparison.Ordinal);
        var itemHasError = ItemHasError(item);
        var addError = !hasErrorClass
            && (itemHasError || (!item.IsExactlyFalse("error") && parameters.Truthy("errorMessage") && !anyItemHasError));
        if (addError)
        {
            itemClasses = "govuk-input--error";
        }

        if (string.IsNullOrEmpty(classesText) || !classesText.Contains("govuk-input--width-", StringComparison.Ordinal))
        {
            itemClasses = string.IsNullOrEmpty(itemClasses)
                ? "govuk-input--width-" + itemWidth
                : itemClasses + " govuk-input--width-" + itemWidth;
        }

        if (item.Truthy("classes"))
        {
            itemClasses = string.IsNullOrEmpty(itemClasses)
                ? item.Print("classes")!
                : itemClasses + " " + item.Print("classes");
        }

        return itemClasses;
    }

    private static string? ResolveItemValue(
        ParamBag item,
        string? itemValue,
        ParamBag parameters,
        string namePrefix,
        string itemName)
    {
        if (itemValue is not null)
        {
            return itemValue;
        }

        var values = parameters.Get("values");
        var key = namePrefix + itemName;
        if (values.Has(key))
        {
            return values.Print(key);
        }

        return values.Print(itemName);
    }

    private static JsonObject BuildInputParams(
        ParamBag item,
        string inputId,
        string name,
        string? value,
        string itemClasses,
        string? labelText)
    {
        var properties = new JsonObject
        {
            ["label"] = new JsonObject
            {
                ["text"] = labelText,
                ["classes"] = "govuk-date-input__label",
            },
            ["id"] = inputId,
            ["name"] = name,
            ["type"] = "text",
            ["classes"] = string.IsNullOrEmpty(itemClasses)
                ? "govuk-date-input__input"
                : "govuk-date-input__input " + itemClasses,
            ["inputmode"] = item.Truthy("inputmode") ? item.Print("inputmode") : "numeric",
        };
        if (value is not null)
        {
            properties["value"] = value;
        }

        if (item.Truthy("autocomplete"))
        {
            properties["autocomplete"] = item.Print("autocomplete");
        }

        if (item.Truthy("pattern"))
        {
            properties["pattern"] = item.Print("pattern");
        }

        if (item.Has("attributes"))
        {
            properties["attributes"] = JsonNode.Parse(item.Get("attributes").JsonRoot.GetRawText());
        }

        return properties;
    }

    private static string ResolveLabelText(ParamBag item, string itemName)
    {
        var label = item.Get("label");
        if (label.IsUndefined)
        {
            return Capitalize(itemName);
        }

        if (label.JsonRoot.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            return label.Scalar() ?? Capitalize(itemName);
        }

        if (label.Truthy("text"))
        {
            return label.Print("text") ?? Capitalize(itemName);
        }

        return Capitalize(itemName);
    }

    private static string Capitalize(string? value) =>
        string.IsNullOrEmpty(value) ? "" : char.ToUpperInvariant(value[0]) + value[1..];

    private static void TrimTrailingNewline(StringBuilder builder)
    {
        if (builder.Length > 0 && builder[^1] == '\n')
        {
            builder.Length -= 1;
        }
    }

    private sealed record DateDefaults(ParamBag Day, ParamBag Month, ParamBag Year);
}
