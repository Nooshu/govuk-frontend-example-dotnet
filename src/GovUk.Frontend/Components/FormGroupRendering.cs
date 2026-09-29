using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

internal static class FormGroupRendering
{
    internal static string FormGroupClass(ParamBag parameters)
    {
        var classes = "govuk-form-group";
        if (parameters.Truthy("errorMessage"))
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

    internal static string FormGroupAttributes(ParamBag parameters) =>
        parameters.Get("formGroup").Attributes();

    internal static string RenderHintBlock(string hintId, ParamBag hint)
    {
        var hintParams = WithId(hint, hintId);
        return Nj.Indent(new HintRenderer().Render(hintParams).Trim(), 2);
    }

    internal static string RenderErrorBlock(string errorId, ParamBag errorMessage)
    {
        var errorParams = WithId(errorMessage, errorId);
        return Nj.Indent(new ErrorMessageRenderer().Render(errorParams).Trim(), 2);
    }

    internal static string RenderFormGroupInsert(ParamBag insert, int indentWidth)
    {
        if (insert.Truthy("html"))
        {
            return Nj.Indent((insert.Print("html") ?? "").Trim(), indentWidth);
        }

        if (insert.Truthy("text"))
        {
            return Nj.Escape(insert.Print("text"));
        }

        return "";
    }

    internal static string AppendDescribedBy(string describedBy, string id) =>
        string.IsNullOrEmpty(describedBy) ? id : describedBy + " " + id;

    internal static ParamBag WithId(ParamBag source, string id)
    {
        var properties = ToJsonObject(source);
        properties["id"] = id;
        return ParamBagFromJsonObject(properties);
    }

    internal static ParamBag WithFor(ParamBag source, string forId)
    {
        var properties = ToJsonObject(source);
        properties["for"] = forId;
        return ParamBagFromJsonObject(properties);
    }

    internal static ParamBag Merge(JsonObject properties) => ParamBagFromJsonObject(properties);

    internal static bool ValuesContains(ParamBag parameters, string? value)
    {
        foreach (var entry in parameters.Items("values"))
        {
            if (entry.Scalar() == value)
            {
                return true;
            }
        }

        return false;
    }

    internal static bool RadioIsChecked(ParamBag parameters, ParamBag item)
    {
        if (item.Truthy("checked"))
        {
            return true;
        }

        if (!parameters.Truthy("value"))
        {
            return false;
        }

        if (item.Print("value") != parameters.Print("value"))
        {
            return false;
        }

        return !item.IsExactlyFalse("checked");
    }

    internal static bool CheckboxIsChecked(ParamBag parameters, ParamBag item)
    {
        if (item.Truthy("checked"))
        {
            return true;
        }

        if (!parameters.Has("values"))
        {
            return false;
        }

        if (!ValuesContains(parameters, item.Print("value")))
        {
            return false;
        }

        return !item.IsExactlyFalse("checked");
    }

    internal static string ItemId(string idPrefix, int index, ParamBag item) =>
        item.Truthy("id") ? item.Print("id")! : idPrefix + (index > 1 ? "-" + index : "");

    private static JsonObject ToJsonObject(ParamBag source) =>
        JsonNode.Parse(source.JsonRoot.GetRawText()) as JsonObject ?? new JsonObject();

    private static ParamBag ParamBagFromJsonObject(JsonObject properties)
    {
        using var document = System.Text.Json.JsonDocument.Parse(properties.ToJsonString());
        return ParamBag.FromJson(document.RootElement.Clone());
    }
}
