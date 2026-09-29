using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class EmailModel : PageModel
{
    public string? Email { get; private set; }

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/email");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Email = draft.Email;
        return Page();
    }

    public IActionResult OnPost(string? email)
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/email");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Email = email;
        Error = LicenceJourney.ValidateEmail(email);
        if (Error is not null)
        {
            return Page();
        }

        draft.Email = email!.Trim();
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/check");
    }
}
