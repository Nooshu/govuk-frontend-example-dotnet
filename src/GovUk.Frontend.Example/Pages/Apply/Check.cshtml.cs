using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class CheckModel : PageModel
{
    public string? LengthLabel { get; private set; }

    public string? FullName { get; private set; }

    public string? DateOfBirth { get; private set; }

    public string? Country { get; private set; }

    public string? Email { get; private set; }

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var missing = LicenceJourney.MissingStep(draft);
        if (missing is not null)
        {
            return Redirect(missing);
        }

        Bind(draft);
        return Page();
    }

    public IActionResult OnPost()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var missing = LicenceJourney.MissingStep(draft);
        if (missing is not null)
        {
            return Redirect(missing);
        }

        draft.Reference ??= LicenceJourney.CreateReference();
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/confirmation");
    }

    private void Bind(LicenceDraft draft)
    {
        LengthLabel = LicenceJourney.LengthLabel(draft.Length);
        FullName = draft.FullName;
        DateOfBirth = $"{draft.Day} {draft.Month} {draft.Year}";
        Country = draft.Country;
        Email = draft.Email;
    }
}
