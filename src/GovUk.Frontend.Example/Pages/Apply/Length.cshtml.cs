using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class LengthModel : PageModel
{
    public string? Length { get; private set; }

    public string? Error { get; private set; }

    public void OnGet()
    {
        Length = LicenceSession.Load(HttpContext.Session).Length;
    }

    public IActionResult OnPost(string? length)
    {
        Error = LicenceJourney.ValidateLength(length);
        Length = length;
        if (Error is not null)
        {
            return Page();
        }

        var draft = LicenceSession.Load(HttpContext.Session);
        draft.Length = length;
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/name");
    }
}
