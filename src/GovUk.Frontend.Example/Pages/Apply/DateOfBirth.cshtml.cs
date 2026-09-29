using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages.Apply;

[ResponseCacheKind("sensitive-document")]
public class DateOfBirthModel : PageModel
{
    public string? Day { get; private set; }

    public string? Month { get; private set; }

    public string? Year { get; private set; }

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(draft, "/apply/date-of-birth");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Day = draft.Day;
        Month = draft.Month;
        Year = draft.Year;
        return Page();
    }

    public IActionResult OnPost(
        [FromForm(Name = "date-of-birth-day")] string? day,
        [FromForm(Name = "date-of-birth-month")] string? month,
        [FromForm(Name = "date-of-birth-year")] string? year)
    {
        var draft = LicenceSession.Load(HttpContext.Session);
        var earlier = LicenceJourney.EarlierMissing(
            new LicenceDraft { Length = draft.Length, FullName = draft.FullName },
            "/apply/date-of-birth");
        if (earlier is not null)
        {
            return Redirect(earlier);
        }

        Day = day;
        Month = month;
        Year = year;
        Error = LicenceJourney.ValidateDateOfBirth(day, month, year, DateOnly.FromDateTime(DateTime.UtcNow));
        if (Error is not null)
        {
            return Page();
        }

        draft.Day = day!.Trim();
        draft.Month = month!.Trim();
        draft.Year = year!.Trim();
        LicenceSession.Save(HttpContext.Session, draft);
        return Redirect("/apply/country");
    }
}
