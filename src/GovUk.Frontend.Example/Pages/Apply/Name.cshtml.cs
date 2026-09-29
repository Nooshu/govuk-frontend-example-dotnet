using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class NameModel : PageModel
{
    public string? FullName { get; private set; }

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/name");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        FullName = draft.FullName;
        return Page();
    }

    public IActionResult OnPost(string? fullName)
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/name");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Error = LicenceJourney.ValidateName(fullName);
        FullName = fullName;
        if (Error is not null)
        {
            return Page();
        }

        draft.FullName = fullName!.Trim();
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/date-of-birth");
    }
}
