using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class CountryModel : PageModel
{
    public string? Country { get; private set; }

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/country");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Country = draft.Country;
        return Page();
    }

    public IActionResult OnPost(string? country)
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/country");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Country = country;
        Error = LicenceJourney.ValidateCountry(country);
        if (Error is not null)
        {
            return Page();
        }

        draft.Country = country;
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/email");
    }
}
