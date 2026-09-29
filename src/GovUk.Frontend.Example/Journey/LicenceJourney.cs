using System.Globalization;

namespace GovUk.Frontend.Example.Journey;

public static class LicenceJourney
{
    public const string SessionKey = "fishing-licence";

    public static readonly IReadOnlyList<(string Value, string Label)> Lengths =
    [
        ("1-day", "1 day"),
        ("8-days", "8 days"),
        ("12-months", "12 months"),
    ];

    public static readonly IReadOnlyList<string> Countries = ["England", "Wales", "Scotland"];

    public static string? LengthLabel(string? value) =>
        Lengths.FirstOrDefault(length => length.Value == value).Label;

    private static readonly string[] Steps =
    [
        "/apply/length",
        "/apply/name",
        "/apply/date-of-birth",
        "/apply/country",
        "/apply/email",
    ];

    public static string? EarlierMissing(LicenceDraft draft, string step)
    {
        var missing = MissingStep(draft);
        if (missing is null)
        {
            return null;
        }

        var missingIndex = Array.IndexOf(Steps, missing);
        var stepIndex = Array.IndexOf(Steps, step);
        return missingIndex >= 0 && stepIndex >= 0 && missingIndex < stepIndex ? missing : null;
    }

    public static string? MissingStep(LicenceDraft draft)
    {
        if (string.IsNullOrEmpty(draft.Length))
        {
            return "/apply/length";
        }

        if (string.IsNullOrEmpty(draft.FullName))
        {
            return "/apply/name";
        }

        if (string.IsNullOrEmpty(draft.Day) || string.IsNullOrEmpty(draft.Month) || string.IsNullOrEmpty(draft.Year))
        {
            return "/apply/date-of-birth";
        }

        if (string.IsNullOrEmpty(draft.Country))
        {
            return "/apply/country";
        }

        if (string.IsNullOrEmpty(draft.Email))
        {
            return "/apply/email";
        }

        return null;
    }

    public static string? ValidateLength(string? value) =>
        Lengths.Any(length => length.Value == value) ? null : "Select how long you need the licence for";

    public static string? ValidateName(string? value)
    {
        var name = value?.Trim() ?? "";
        if (name.Length == 0)
        {
            return "Enter your full name";
        }

        if (name.Length < 2)
        {
            return "Enter your full name";
        }

        return null;
    }

    public static string? ValidateDateOfBirth(string? day, string? month, string? year, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(month) || string.IsNullOrWhiteSpace(year))
        {
            return "Enter your date of birth";
        }

        if (!int.TryParse(day, NumberStyles.None, CultureInfo.InvariantCulture, out var dayNumber)
            || !int.TryParse(month, NumberStyles.None, CultureInfo.InvariantCulture, out var monthNumber)
            || !int.TryParse(year, NumberStyles.None, CultureInfo.InvariantCulture, out var yearNumber)
            || yearNumber is < 1000 or > 9999)
        {
            return "Enter a real date of birth";
        }

        DateOnly date;
        try
        {
            date = new DateOnly(yearNumber, monthNumber, dayNumber);
        }
        catch (ArgumentOutOfRangeException)
        {
            return "Enter a real date of birth";
        }

        if (date > today)
        {
            return "Date of birth must be in the past";
        }

        var age = today.Year - date.Year;
        if (date > today.AddYears(-age))
        {
            age--;
        }

        if (age < 13)
        {
            return "You must be at least 13 to use this example";
        }

        return null;
    }

    public static string? ValidateCountry(string? value) =>
        Countries.Contains(value, StringComparer.Ordinal) ? null : "Select where you will fish";

    public static string? ValidateEmail(string? value)
    {
        var email = value?.Trim() ?? "";
        var at = email.IndexOf('@');
        if (at <= 0 || at != email.LastIndexOf('@') || !email[(at + 1)..].Contains('.', StringComparison.Ordinal))
        {
            return "Enter an email address in the correct format, like name@example.com";
        }

        return null;
    }

    public static string CreateReference()
    {
        var number = Random.Shared.Next(0, 100_000_000);
        return "FR" + number.ToString("00000000", CultureInfo.InvariantCulture);
    }
}
