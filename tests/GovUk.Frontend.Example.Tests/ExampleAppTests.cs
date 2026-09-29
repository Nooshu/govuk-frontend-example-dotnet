using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using GovUk.Frontend.Example.Catalogue;
using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Journey;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GovUk.Frontend.Example.Tests;

public class ExampleAppTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExampleAppTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_lists_links_only()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Apply for a fishing rod licence", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/components/button\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("govuk-button", html, StringComparison.Ordinal);
        Assert.Contains("Content-Security-Policy", response.Headers.Select(header => header.Key));
        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
        Assert.False(response.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Catalogue_renders_one_fixture_and_a_raw_fragment()
    {
        var client = _factory.CreateClient();
        var page = await client.GetStringAsync("/components/button?fixture=default");
        Assert.Contains("Current version: default", page, StringComparison.Ordinal);
        Assert.Contains("HTML matches the fixture", page, StringComparison.Ordinal);
        Assert.Contains("The C# HTML is the same as the official fixture HTML.", page, StringComparison.Ordinal);
        Assert.DoesNotContain("macro output", page, StringComparison.Ordinal);
        Assert.Contains("app-component-preview__frame", page, StringComparison.Ordinal);
        Assert.Contains("Versions (Fixtures)", page, StringComparison.Ordinal);
        Assert.Contains("aria-current=\"true\">default", page, StringComparison.Ordinal);
        Assert.Contains("This is a live demo. It is not a real government service.", page, StringComparison.Ordinal);
        var raw = await client.GetStringAsync("/components/button/raw?fixture=default");
        Assert.StartsWith("<button", raw.Trim(), StringComparison.Ordinal);
        Assert.DoesNotContain("<html", raw, StringComparison.OrdinalIgnoreCase);
        var missing = await client.GetAsync("/components/not-a-component/raw");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        var unknownPage = await client.GetStringAsync("/components/not-a-component");
        Assert.Contains("Component not found", unknownPage, StringComparison.Ordinal);
        var unknownFixture = await client.GetStringAsync("/components/button?fixture=does-not-exist");
        Assert.DoesNotContain("HTML matches the fixture", unknownFixture, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Hidden_fixtures_are_listed()
    {
        var client = _factory.CreateClient();
        var page = await client.GetStringAsync("/components/back-link");
        Assert.Contains("Back link", page, StringComparison.Ordinal);
        Assert.Contains("html as text", page, StringComparison.Ordinal);
        Assert.Contains("govuk-tag", page, StringComparison.Ordinal);
        Assert.Contains("Current", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Catalogue_banner_describes_the_rendered_fixture_html()
    {
        var catalog = new FixtureCatalog(RepoPaths.ComponentFixturesDirectory());
        var fixture = catalog.Find("button", "default");
        Assert.NotNull(fixture);

        var client = _factory.CreateClient();
        var page = await client.GetStringAsync("/components/button?fixture=default");
        Assert.Contains(fixture.Html, page, StringComparison.Ordinal);
        Assert.Contains("The C# HTML is the same as the official fixture HTML.", page, StringComparison.Ordinal);
        Assert.DoesNotContain("The C# HTML is different from the official fixture HTML.", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Search_engines_are_told_not_to_index()
    {
        var client = _factory.CreateClient();
        var home = await client.GetAsync("/");
        var homeHtml = await home.Content.ReadAsStringAsync();
        Assert.Equal("noindex, nofollow", home.Headers.GetValues("X-Robots-Tag").Single());
        Assert.Contains("<meta name=\"robots\" content=\"noindex, nofollow\">", homeHtml, StringComparison.Ordinal);

        var health = await client.GetAsync("/health");
        Assert.Equal("noindex, nofollow", health.Headers.GetValues("X-Robots-Tag").Single());

        var raw = await client.GetAsync("/components/button/raw?fixture=default");
        Assert.Equal("noindex, nofollow", raw.Headers.GetValues("X-Robots-Tag").Single());

        var stylesheet = System.Text.RegularExpressions.Regex.Match(homeHtml, "href=\"(/assets/[^\"]+\\.css)\"").Groups[1].Value;
        var asset = await client.GetAsync(stylesheet);
        Assert.Equal("noindex, nofollow", asset.Headers.GetValues("X-Robots-Tag").Single());

        var robots = await client.GetAsync("/robots.txt");
        var body = await robots.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, robots.StatusCode);
        Assert.Contains("User-agent: *", body, StringComparison.Ordinal);
        Assert.Contains("Disallow:", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Disallow: /", body, StringComparison.Ordinal);
        Assert.Equal("noindex, nofollow", robots.Headers.GetValues("X-Robots-Tag").Single());
    }

    [Fact]
    public async Task Health_and_assets_use_baseline_cache_kinds()
    {
        var client = _factory.CreateClient();
        var health = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal("no-store", health.Headers.CacheControl?.NoStore == true ? "no-store" : "");
        var home = await client.GetStringAsync("/");
        var stylesheet = System.Text.RegularExpressions.Regex.Match(home, "href=\"(/assets/[^\"]+\\.css)\"").Groups[1].Value;
        var asset = await client.GetAsync(stylesheet);
        Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
        Assert.Contains("immutable", asset.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Https_forwarding_sends_hsts()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Forwarded-Proto", "https");
        var response = await client.SendAsync(request);
        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Journey_rejects_a_blank_answer_then_reaches_confirmation()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        Assert.Equal(HttpStatusCode.Found, (await client.GetAsync("/apply/name")).StatusCode);

        var invalid = await client.PostAsync("/apply/length", await FormWithToken(client, "/apply/length", ("length", "")));
        var invalidHtml = await invalid.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
        Assert.Contains("There is a problem", invalidHtml, StringComparison.Ordinal);
        Assert.Contains("no-store", invalid.Headers.CacheControl?.ToString(), StringComparison.Ordinal);

        await Follow(client, "/apply/length", ("length", "1-day"));
        var shortName = await client.PostAsync("/apply/name", await FormWithToken(client, "/apply/name", ("fullName", "A")));
        Assert.Contains("Enter your full name", await shortName.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await Follow(client, "/apply/name", ("fullName", "Ada Lovelace"));
        var badDate = await client.PostAsync(
            "/apply/date-of-birth",
            await FormWithToken(client, "/apply/date-of-birth", ("date-of-birth-day", "31"), ("date-of-birth-month", "2"), ("date-of-birth-year", "2000")));
        Assert.Contains("Enter a real date of birth", await badDate.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await Follow(client, "/apply/date-of-birth", ("date-of-birth-day", "10"), ("date-of-birth-month", "12"), ("date-of-birth-year", "1815"));
        var badCountry = await client.PostAsync("/apply/country", await FormWithToken(client, "/apply/country", ("country", "France")));
        Assert.Contains("Select where you will fish", await badCountry.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await Follow(client, "/apply/country", ("country", "England"));
        var badEmail = await client.PostAsync("/apply/email", await FormWithToken(client, "/apply/email", ("email", "ada")));
        Assert.Contains("correct format", await badEmail.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        await Follow(client, "/apply/email", ("email", "ada@example.com"));

        var check = await client.GetStringAsync("/apply/check");
        Assert.Contains("Ada Lovelace", check, StringComparison.Ordinal);
        var submitted = await client.PostAsync("/apply/check", await FormWithToken(client, "/apply/check"));
        Assert.Equal("/apply/confirmation", submitted.Headers.Location?.OriginalString);
        var done = await client.GetStringAsync("/apply/confirmation");
        Assert.Contains("Application complete", done, StringComparison.Ordinal);
        Assert.Contains("FR", done, StringComparison.Ordinal);

        var again = await client.GetAsync("/apply/confirmation");
        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
    }

    [Fact]
    public async Task Example_sections_from_the_footer_are_pages()
    {
        var client = _factory.CreateClient();
        var help = await client.GetStringAsync("/help");
        Assert.Contains("Help", help, StringComparison.Ordinal);
        var fallback = await client.GetStringAsync("/example-section");
        Assert.Contains("This example", fallback, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unknown_path_is_not_found_and_start_page_is_served()
    {
        var client = _factory.CreateClient();
        var missing = await client.GetAsync("/not-a-page");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var start = await client.GetAsync("/apply");
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        Assert.Contains("Start now", await start.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Later_pages_redirect_until_earlier_answers_exist()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        var tokenPage = await client.GetStringAsync("/apply/length");
        var token = System.Text.RegularExpressions.Regex.Match(
            tokenPage,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        FormUrlEncodedContent Form(params (string Name, string Value)[] fields)
        {
            var data = fields.ToDictionary(field => field.Name, field => field.Value);
            data["__RequestVerificationToken"] = token;
            return new FormUrlEncodedContent(data);
        }

        Assert.Equal("/apply/length", (await client.GetAsync("/apply/name")).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.PostAsync("/apply/name", Form(("fullName", "Ada")))).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.GetAsync("/apply/date-of-birth")).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.PostAsync("/apply/date-of-birth", Form(("day", "1"), ("month", "1"), ("year", "2000")))).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.GetAsync("/apply/country")).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.PostAsync("/apply/country", Form(("country", "England")))).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.GetAsync("/apply/email")).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.PostAsync("/apply/email", Form(("email", "ada@example.com")))).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.GetAsync("/apply/check")).Headers.Location?.OriginalString);
        Assert.Equal("/apply/length", (await client.PostAsync("/apply/check", Form())).Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Confirmation_without_a_reference_returns_to_the_start()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        var response = await client.GetAsync("/apply/confirmation");
        Assert.Equal("/apply", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Session_round_trips_a_draft()
    {
        var session = new DictionarySession();
        Assert.Null(LicenceSession.Load(session).Length);
        session.SetString(LicenceJourney.SessionKey, "null");
        Assert.Null(LicenceSession.Load(session).Length);
        var draft = new LicenceDraft { Length = "1-day" };
        LicenceSession.Save(session, draft);
        Assert.Equal("1-day", LicenceSession.Load(session).Length);
        Assert.Equal("1-day", JsonSerializer.Deserialize<LicenceDraft>(session.GetString(LicenceJourney.SessionKey)!)!.Length);
    }

    private static async Task Follow(HttpClient client, string path, params (string Name, string Value)[] fields)
    {
        var response = await client.PostAsync(path, await FormWithToken(client, path, fields));
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static async Task<FormUrlEncodedContent> FormWithToken(
        HttpClient client,
        string path,
        params (string Name, string Value)[] fields)
    {
        var html = await client.GetStringAsync(path);
        var token = System.Text.RegularExpressions.Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        if (token.Length == 0)
        {
            token = System.Text.RegularExpressions.Regex.Match(
                html,
                "value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"").Groups[1].Value;
        }

        var data = fields.ToDictionary(field => field.Name, field => field.Value);
        data["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(data);
    }

    private sealed class DictionarySession : ISession
    {
        private readonly Dictionary<string, byte[]> _values = [];

        public bool IsAvailable => true;

        public string Id => "test";

        public IEnumerable<string> Keys => _values.Keys;

        public void Clear() => _values.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _values.Remove(key);

        public void Set(string key, byte[] value) => _values[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_values.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }

            value = [];
            return false;
        }
    }
}
