using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

public class StartModel : PageModel
{
    public void OnGet() => ViewData["CookieChoice"] = CookieNotice.Read(HttpContext.Session);

    public IActionResult OnPost(string? cookies)
    {
        CookieNotice.TrySave(HttpContext.Session, cookies);
        return Redirect("/apply");
    }
}
