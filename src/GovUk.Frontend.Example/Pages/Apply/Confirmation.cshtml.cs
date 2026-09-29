using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class ConfirmationModel : PageModel
{
    public string Reference { get; private set; } = "";

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        if (string.IsNullOrEmpty(draft.Reference))
        {
            return Redirect("/apply");
        }

        Reference = draft.Reference;
        return Page();
    }
}
