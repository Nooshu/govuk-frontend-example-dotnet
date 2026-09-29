using System.Text.Json;
using System.Text.Json.Nodes;
using GovUk.Frontend.Components;
using Microsoft.AspNetCore.Html;

namespace GovUk.Frontend.Example;

public static class GovUkHtml
{
    public static IHtmlContent Component(string name, JsonObject? options = null) =>
        new HtmlString(Render(name, options));

    public static string Render(string name, JsonObject? options = null)
    {
        options ??= new JsonObject();
        using var document = JsonDocument.Parse(options.ToJsonString());
        return ComponentCatalog.Render(name, document.RootElement);
    }
}
