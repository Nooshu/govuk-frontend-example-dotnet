using GovUk.Frontend.Example.Journey;

namespace GovUk.Frontend.Example.Tests;

public class LicenceJourneyTests
{
    [Fact]
    public void Missing_step_follows_the_journey_order()
    {
        var draft = new LicenceDraft();
        Assert.Equal("/apply/length", LicenceJourney.MissingStep(draft));
        draft.Length = "1-day";
        Assert.Equal("/apply/name", LicenceJourney.MissingStep(draft));
        draft.FullName = "Ada Lovelace";
        Assert.Equal("/apply/date-of-birth", LicenceJourney.MissingStep(draft));
        draft.Day = "10";
        draft.Month = "12";
        draft.Year = "1815";
        Assert.Equal("/apply/country", LicenceJourney.MissingStep(draft));
        draft.Country = "England";
        Assert.Equal("/apply/email", LicenceJourney.MissingStep(draft));
        draft.Email = "ada@example.com";
        Assert.Null(LicenceJourney.MissingStep(draft));
    }

    [Fact]
    public void Earlier_missing_ignores_later_steps_and_unknown_paths()
    {
        var draft = new LicenceDraft { Length = "1-day", FullName = "Ada Lovelace" };
        Assert.Null(LicenceJourney.EarlierMissing(draft, "/apply/date-of-birth"));
        Assert.Equal("/apply/length", LicenceJourney.EarlierMissing(new LicenceDraft(), "/apply/name"));
        Assert.Null(LicenceJourney.EarlierMissing(new LicenceDraft { Email = "ada@example.com" }, "/unknown"));
        draft.Day = "10";
        draft.Month = "12";
        draft.Year = "1815";
        draft.Country = "England";
        draft.Email = "ada@example.com";
        Assert.Null(LicenceJourney.EarlierMissing(draft, "/apply/email"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("forever")]
    public void Length_is_required(string? value)
    {
        Assert.Equal("Select how long you need the licence for", LicenceJourney.ValidateLength(value));
    }

    [Fact]
    public void Length_accepts_a_known_choice()
    {
        Assert.Null(LicenceJourney.ValidateLength("8-days"));
        Assert.Equal("8 days", LicenceJourney.LengthLabel("8-days"));
        Assert.Null(LicenceJourney.LengthLabel("nope"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("A")]
    public void Name_must_be_at_least_two_characters(string? value)
    {
        Assert.Equal("Enter your full name", LicenceJourney.ValidateName(value));
    }

    [Fact]
    public void Name_accepts_a_full_name()
    {
        Assert.Null(LicenceJourney.ValidateName(" Ada Lovelace "));
    }

    [Fact]
    public void Date_of_birth_rejects_incomplete_unreal_future_and_young_dates()
    {
        var today = new DateOnly(2026, 9, 29);
        Assert.Equal("Enter your date of birth", LicenceJourney.ValidateDateOfBirth("", "1", "2000", today));
        Assert.Equal("Enter a real date of birth", LicenceJourney.ValidateDateOfBirth("aa", "1", "2000", today));
        Assert.Equal("Enter a real date of birth", LicenceJourney.ValidateDateOfBirth("31", "2", "2000", today));
        Assert.Equal("Enter a real date of birth", LicenceJourney.ValidateDateOfBirth("1", "1", "999", today));
        Assert.Equal("Date of birth must be in the past", LicenceJourney.ValidateDateOfBirth("30", "9", "2026", today));
        Assert.Equal("You must be at least 13 to use this example", LicenceJourney.ValidateDateOfBirth("29", "9", "2014", today));
        Assert.Null(LicenceJourney.ValidateDateOfBirth("10", "12", "1815", today));
    }

    [Fact]
    public void Date_of_birth_counts_a_birthday_later_this_year()
    {
        var today = new DateOnly(2026, 9, 29);
        Assert.Equal(
            "You must be at least 13 to use this example",
            LicenceJourney.ValidateDateOfBirth("30", "9", "2013", today));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("France")]
    public void Country_must_be_one_of_the_example_choices(string? value)
    {
        Assert.Equal("Select where you will fish", LicenceJourney.ValidateCountry(value));
    }

    [Fact]
    public void Country_accepts_england()
    {
        Assert.Null(LicenceJourney.ValidateCountry("England"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("ada")]
    [InlineData("ada@example")]
    [InlineData("@example.com")]
    [InlineData("ada@@example.com")]
    public void Email_must_look_like_an_address(string? value)
    {
        Assert.Equal(
            "Enter an email address in the correct format, like name@example.com",
            LicenceJourney.ValidateEmail(value));
    }

    [Fact]
    public void Email_accepts_a_simple_address()
    {
        Assert.Null(LicenceJourney.ValidateEmail(" ada@example.com "));
    }

    [Fact]
    public void Reference_has_a_fixed_shape()
    {
        var reference = LicenceJourney.CreateReference();
        Assert.Matches("^FR[0-9]{8}$", reference);
    }
}
