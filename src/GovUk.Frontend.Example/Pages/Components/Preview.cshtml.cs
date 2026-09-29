using GovUk.Frontend.Components;
using GovUk.Frontend.Example.Catalogue;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Components;

public class PreviewModel(FixtureCatalog catalog) : PageModel
{
    public string Component { get; private set; } = "";

    public bool Found { get; private set; }

    public IReadOnlyList<FixtureExample> Fixtures { get; private set; } = [];

    public FixtureExample? Selected { get; private set; }

    public bool Matches { get; private set; }

    public IHtmlContent? PreviewHtml { get; private set; }

    public string Title => ComponentInfo.Title(Component);

    public string DesignSystemUrl => ComponentInfo.DesignSystemUrl(Component);

    public void OnGet(string name, string? fixture)
    {
        Component = name;
        Found = catalog.TryGet(name, out var fixtures);
        if (!Found)
        {
            return;
        }

        Fixtures = fixtures;
        Selected = catalog.Find(name, fixture);
        if (Selected is null)
        {
            return;
        }

        var html = ComponentCatalog.Render(name, Selected.Options);
        Matches = string.Equals(html, Selected.Html, StringComparison.Ordinal);
        PreviewHtml = new HtmlString(html);
    }
}
