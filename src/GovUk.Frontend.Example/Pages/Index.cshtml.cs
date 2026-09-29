using GovUk.Frontend.Example.Catalogue;
using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages;

public class IndexModel(FixtureCatalog catalog) : PageModel
{
    public IReadOnlyList<string> Components { get; private set; } = [];

    public void OnGet()
    {
        Components = catalog.Names;
        ViewData["CookieChoice"] = CookieNotice.Read(HttpContext.Session);
    }

    public IActionResult OnPost(string? cookies)
    {
        CookieNotice.TrySave(HttpContext.Session, cookies);
        return Redirect(ServiceHome.CataloguePath);
    }
}
