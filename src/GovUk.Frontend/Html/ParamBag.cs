using System.Text;
using System.Text.Json;

namespace GovUk.Frontend.Html;

/// <summary>
/// One GOV.UK macro parameter object. Property order matches the JSON object,
/// which is the order <c>govukAttributes</c> emits.
/// </summary>
public sealed class ParamBag
{
    public static ParamBag Undefined { get; } = new(default);

    private readonly JsonElement _element;
    private readonly bool _undefined;

    private ParamBag(JsonElement element)
    {
        _element = element;
        _undefined = element.ValueKind is JsonValueKind.Undefined;
    }

    public static ParamBag FromJson(JsonElement element) => new(element.Clone());

    internal JsonElement JsonRoot => _element;

    public bool IsUndefined => _undefined;

    public bool IsTruthy => !_undefined && IsTruthyElement(_element);

    public bool Has(string name) => TryGet(name, out _);

    public bool Truthy(string name) => TryGet(name, out var value) && IsTruthyElement(value);

    public ParamBag Get(string name) =>
        TryGet(name, out var value) ? new ParamBag(value.Clone()) : Undefined;

    public IReadOnlyList<ParamBag> Items(string name)
    {
        if (!TryGet(name, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return EnumerateArray(value);
    }

    /// <summary>When this bag wraps a JSON array, yields one bag per element.</summary>
    public IEnumerable<ParamBag> EnumerateArray()
    {
        if (_undefined || _element.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var item in _element.EnumerateArray())
        {
            yield return new ParamBag(item.Clone());
        }
    }

    private static IReadOnlyList<ParamBag> EnumerateArray(JsonElement value)
    {
        var items = new List<ParamBag>(value.GetArrayLength());
        foreach (var item in value.EnumerateArray())
        {
            items.Add(new ParamBag(item.Clone()));
        }

        return items;
    }

    public IEnumerable<(string Name, string? Value)> Pairs()
    {
        if (_undefined || _element.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var property in _element.EnumerateObject())
        {
            yield return (property.Name, PrintElement(property.Value));
        }
    }

    public int Length(string name) =>
        TryGet(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;

    /// <summary>
    /// Value as Nunjucks would print it, or null when the key is missing or JSON null.
    /// </summary>
    public string? Print(string name) =>
        TryGet(name, out var value) ? PrintElement(value) : null;

    public string Escaped(string name) => Nj.Escape(Print(name));

    /// <summary>
    /// Nunjucks 3 <c>default</c>. The second argument <c>true</c> replaces every falsy
    /// value (<paramref name="whenFalsy"/>). Otherwise only a missing key is replaced.
    /// </summary>
    public string Default(string name, string fallback, bool whenFalsy = false)
    {
        if (!TryGet(name, out var value))
        {
            return fallback;
        }

        if (whenFalsy && !IsTruthyElement(value))
        {
            return fallback;
        }

        return PrintElement(value) ?? "";
    }

    /// <summary>True when the value is the boolean <c>false</c>. Missing is not false.</summary>
    public bool IsExactlyFalse(string name) =>
        TryGet(name, out var value) && value.ValueKind == JsonValueKind.False;

    /// <summary>True when the value is the boolean <c>true</c>.</summary>
    public bool IsExactlyTrue(string name) =>
        TryGet(name, out var value) && value.ValueKind == JsonValueKind.True;

    /// <summary>
    /// <c>value !== undefined</c>. A present JSON null still counts as defined.
    /// </summary>
    public bool Defined(string name) => Has(name);

    public string Attributes(string name = "attributes") =>
        TryGet(name, out var value) ? RenderAttributes(value) : "";

    public static string RenderAttributes(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString() ?? "";
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return "";
        }

        var builder = new StringBuilder();
        foreach (var property in element.EnumerateObject())
        {
            AppendAttribute(builder, property.Name, property.Value, optional: false);
        }

        return builder.ToString();
    }

    public string? Scalar() => _undefined ? null : PrintElement(_element);

    /// <summary>Raw JSON for this value (for composing nested components).</summary>
    public string RawJson() =>
        _undefined ? "null" : _element.GetRawText();

    private bool TryGet(string name, out JsonElement value)
    {
        if (_undefined || _element.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }

        return _element.TryGetProperty(name, out value);
    }

    private static void AppendAttribute(
        StringBuilder builder,
        string name,
        JsonElement value,
        bool optional)
    {
        if (value.ValueKind == JsonValueKind.Object
            && value.TryGetProperty("value", out var inner)
            && value.TryGetProperty("optional", out var optionalElement))
        {
            AppendAttribute(builder, name, inner, optionalElement.ValueKind == JsonValueKind.True);
            return;
        }

        if (optional && value.ValueKind == JsonValueKind.True)
        {
            builder.Append(' ').Append(Nj.Escape(name));
            return;
        }

        if (optional && value.ValueKind is JsonValueKind.False or JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        var printed = PrintElement(value) ?? "";
        builder
            .Append(' ')
            .Append(Nj.Escape(name))
            .Append("=\"")
            .Append(Nj.Escape(printed))
            .Append('"');
    }

    private static string? PrintElement(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Number => value.GetRawText(),
        _ => value.GetRawText(),
    };

    private static bool IsTruthyElement(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False or JsonValueKind.Null or JsonValueKind.Undefined => false,
        JsonValueKind.String => !string.IsNullOrEmpty(value.GetString()),
        JsonValueKind.Number => !IsZero(value),
        JsonValueKind.Array or JsonValueKind.Object => true,
        _ => false,
    };

    private static bool IsZero(JsonElement value) =>
        value.TryGetDecimal(out var number) && number == 0;
}
