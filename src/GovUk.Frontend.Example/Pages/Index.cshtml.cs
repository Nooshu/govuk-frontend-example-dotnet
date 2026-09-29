using GovUk.Frontend.Example.Catalogue;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages;

public class IndexModel(FixtureCatalog catalog) : PageModel
{
    public IReadOnlyList<string> Components { get; private set; } = [];

    public void OnGet() => Components = catalog.Names;
}
