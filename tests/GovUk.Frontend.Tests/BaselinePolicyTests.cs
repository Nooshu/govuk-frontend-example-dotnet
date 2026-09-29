using FluentAssertions;
using GovUk.Frontend.Baseline;

namespace GovUk.Frontend.Tests;

public class BaselinePolicyTests
{
    private readonly BaselinePolicy _policy = BaselinePolicy.Load(Path.Combine(FixtureSet.Root, "baseline", "policy.json"));

    private static ResponseHeaderOptions Document(bool secure = true) => new()
    {
        Kind = "document",
        SecureTransport = secure,
    };

    [Fact]
    public void Https_document_matches_the_owasp_baseline()
    {
        var result = _policy.BuildResponseHeaders(Document());
        result.Headers["Content-Type"].Should().Be("text/html; charset=utf-8");
        result.Headers["Cache-Control"].Should().Be("no-cache");
        result.Headers["X-Frame-Options"].Should().Be("DENY");
        result.Headers["Strict-Transport-Security"].Should().Be("max-age=63072000; includeSubDomains");
        result.Headers["Vary"].Should().Be("Accept-Encoding");
        result.Headers["Content-Security-Policy"].Should().Contain("default-src 'self'");
        result.Headers["Content-Security-Policy"].Should().Contain("'sha256-GUQ5ad8JK5KmEWmROf3LZd9ge94daqNvd8xy9YS1iDw='");
        result.Headers["Content-Security-Policy"].Should().Contain("upgrade-insecure-requests");
        result.Headers["Content-Security-Policy"].Should().NotContain("unsafe-inline");
        result.Headers["Permissions-Policy"].Should().Contain("camera=()");
        result.Headers.Should().NotContainKey("Content-Security-Policy-Report-Only");
        result.Remove.Should().Contain(["Server", "X-Powered-By", "Expect-CT", "Public-Key-Pins"]);
    }

    [Fact]
    public void Http_omits_hsts_and_preload_requires_https()
    {
        _policy.BuildResponseHeaders(Document(secure: false)).Headers.Should().NotContainKey("Strict-Transport-Security");
        var preloaded = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            HstsPreload = true,
        });
        preloaded.Headers["Strict-Transport-Security"].Should().Be("max-age=63072000; includeSubDomains; preload");
        var act = () => _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = false,
            HstsPreload = true,
        });
        act.Should().Throw<ArgumentException>().WithMessage("*HSTS preload requires secureTransport*");
    }

    [Fact]
    public void Personal_documents_and_cookies_change_caching()
    {
        var sensitive = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "sensitive-document",
            SecureTransport = true,
            SetsCookie = true,
        });
        sensitive.Headers["Cache-Control"].Should().Be("no-store");
        sensitive.Headers["Content-Security-Policy"].Should().Contain("script-src ");
        var withCookie = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            SetsCookie = true,
        });
        withCookie.Headers["Cache-Control"].Should().Be("private, no-cache");
        var act = () => _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "fingerprinted-asset",
            SecureTransport = true,
            SetsCookie = true,
        });
        act.Should().Throw<ArgumentException>().WithMessage("*Set-Cookie belongs on HTML documents*");
    }

    [Fact]
    public void Assets_downloads_and_content_types_follow_the_policy()
    {
        var css = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "fingerprinted-asset",
            SecureTransport = true,
            ContentType = "text/css",
        });
        css.Headers["Cache-Control"].Should().Be("public, max-age=31536000, immutable");
        css.Headers["Content-Type"].Should().Be("text/css; charset=utf-8");
        css.Headers.Should().NotContainKey("Content-Security-Policy");
        css.Headers["X-Content-Type-Options"].Should().Be("nosniff");

        _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
        }).Headers.Should().NotContainKey("Content-Type");

        var file = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "download",
            SecureTransport = true,
            Filename = "café notes.pdf",
        });
        file.Headers["Cache-Control"].Should().Be("private, no-cache");
        file.Headers["Content-Disposition"].Should().Be(
            "attachment; filename=\"caf_ notes.pdf\"; filename*=UTF-8''caf%C3%A9%20notes.pdf");

        _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "sensitive-download",
            SecureTransport = true,
            Filename = "evidence.bin",
        }).Headers["Cache-Control"].Should().Be("no-store");

        var png = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            ContentType = "image/png",
        });
        png.Headers["Content-Type"].Should().Be("image/png");
        var json = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            ContentType = "application/json",
        });
        json.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public void Callers_can_override_document_isolation_and_extend_csp()
    {
        var nonce = _policy.CreateNonce();
        var html = _policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            ContentType = "text/html; charset=UTF-8",
            ReferrerPolicy = "no-referrer",
            CrossOriginResourcePolicy = "same-site",
            CrossOriginEmbedderPolicy = "credentialless",
            EnforceContentSecurityPolicy = false,
            Vary = ["Cookie"],
            CspSources = new Dictionary<string, IReadOnlyList<string>>
            {
                ["img-src"] = ["https://images.example.com"],
            },
            ScriptHashes = ["sha256-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa="],
            ScriptNonce = nonce,
            PermissionsAllow = new Dictionary<string, string>
            {
                ["camera"] = "self",
                ["microphone"] = "none",
                ["usb"] = "https://kiosk.example.com",
            },
            Preload = [new PreloadLink("/assets/font.woff2", "font", "font/woff2")],
        });
        html.Headers["Content-Type"].Should().Be("text/html; charset=UTF-8");
        html.Headers["Referrer-Policy"].Should().Be("no-referrer");
        html.Headers["Cross-Origin-Resource-Policy"].Should().Be("same-site");
        html.Headers["Cross-Origin-Embedder-Policy"].Should().Be("credentialless");
        html.Headers.Should().NotContainKey("Content-Security-Policy");
        html.Headers["Content-Security-Policy-Report-Only"].Should().Contain("https://images.example.com");
        html.Headers["Content-Security-Policy-Report-Only"].Should().Contain($"'nonce-{nonce}'");
        html.Headers["Vary"].Should().Be("Accept-Encoding, Cookie");
        html.Headers["Permissions-Policy"].Should().Contain("camera=(self)");
        html.Headers["Permissions-Policy"].Should().Contain("microphone=()");
        html.Headers["Permissions-Policy"].Should().Contain("usb=(https://kiosk.example.com)");
        html.Headers["Link"].Should().Contain("rel=preload; as=font");

        var unsafePolicy = _policy.BuildContentSecurityPolicy(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>>
            {
                ["script-src"] = ["'unsafe-inline'"],
            },
            AllowUnsafeCspSources = true,
        });
        unsafePolicy.Should().Contain("script-src 'self' 'unsafe-inline'");
    }

    [Fact]
    public void Rejects_invalid_header_options()
    {
        void Act(ResponseHeaderOptions options) => _policy.BuildResponseHeaders(options);
        void Csp(ResponseHeaderOptions options) => _policy.BuildContentSecurityPolicy(options);

        var act = () => Act(new ResponseHeaderOptions { Kind = "page", SecureTransport = true });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown response kind*");
        act = () => Act(new ResponseHeaderOptions { Kind = "static-asset", SecureTransport = true, ScriptNonce = "x" });
        act.Should().Throw<ArgumentException>().WithMessage("*scriptNonce applies to HTML documents*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>>(),
        });
        act.Should().Throw<ArgumentException>().WithMessage("*cspSources applies to HTML documents*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            EnforceContentSecurityPolicy = false,
        });
        act.Should().Throw<ArgumentException>().WithMessage("*CSP report-only applies to HTML documents*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            PermissionsAllow = new Dictionary<string, string>(),
        });
        act.Should().Throw<ArgumentException>().WithMessage("*permissionsAllow applies to HTML documents*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "static-asset",
            SecureTransport = true,
            CrossOriginEmbedderPolicy = "require-corp",
        });
        act.Should().Throw<ArgumentException>().WithMessage("*crossOriginEmbedderPolicy applies to HTML documents*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, Filename = "a.pdf" });
        act.Should().Throw<ArgumentException>().WithMessage("*filename applies to download responses*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true });
        act.Should().Throw<ArgumentException>().WithMessage("*download filename is required*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = "" });
        act.Should().Throw<ArgumentException>().WithMessage("*download filename is required*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = "../secret.pdf" });
        act.Should().Throw<ArgumentException>().WithMessage("*must not include a path*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = "a\\b.pdf" });
        act.Should().Throw<ArgumentException>().WithMessage("*must not include a path*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = "foo..bar.pdf" });
        act.Should().Throw<ArgumentException>().WithMessage("*must not include a path*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = "a\"b.pdf" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid download filename*");
        act = () => Act(new ResponseHeaderOptions { Kind = "download", SecureTransport = true, Filename = new string('a', 181) });
        act.Should().Throw<ArgumentException>().WithMessage("*download filename is required*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, ContentType = "text/html\r\nX: 1" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Content-Type*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, ContentType = "" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Content-Type*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, ReferrerPolicy = "unsafe-url" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Referrer-Policy*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, CrossOriginResourcePolicy = "anywhere" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Cross-Origin-Resource-Policy*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, CrossOriginEmbedderPolicy = "maybe" });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Cross-Origin-Embedder-Policy*");
        act = () => Act(new ResponseHeaderOptions { Kind = "document", SecureTransport = true, Vary = ["Cookie\n"] });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid Vary token*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            PermissionsAllow = new Dictionary<string, string> { ["camera"] = "()" },
        });
        act.Should().NotThrow();
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            PermissionsAllow = new Dictionary<string, string> { ["camera"] = "*" },
        });
        act.Should().Throw<ArgumentException>().WithMessage("*invalid permissions allow*");
        act = () => Act(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            PermissionsAllow = new Dictionary<string, string> { ["flashlight"] = "self" },
        });
        act.Should().Throw<ArgumentException>().WithMessage("*unknown permissions policy feature*");

        var csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["child-src"] = ["https://cdn.example"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*unknown CSP directive*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["upgrade-insecure-requests"] = ["'self'"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*does not take sources*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["*"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["http://cdn.example"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["http:"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["data:"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["blob:"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["'unsafe-eval'"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["script-src"] = ["'unsafe-hashes'"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*not allowed on script-src*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["img-src"] = ["https://images.example.com/a b"] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*invalid CSP source*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            CspSources = new Dictionary<string, IReadOnlyList<string>> { ["img-src"] = [""] },
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*invalid CSP source*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            ScriptNonce = "short",
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*scriptNonce*");
        csp = () => Csp(new ResponseHeaderOptions
        {
            Kind = "document",
            SecureTransport = true,
            ScriptHashes = ["md5-abc"],
        });
        csp.Should().Throw<ArgumentException>().WithMessage("*invalid script hash*");
    }

    [Fact]
    public void Preload_vary_etag_and_clear_site_data_match_the_baseline()
    {
        BaselinePolicy.StrongEtag("hello").Should().StartWith("\"").And.EndWith("\"");
        BaselinePolicy.StrongEtag("hello"u8.ToArray()).Should().Be(BaselinePolicy.StrongEtag("hello"));
        _policy.CreateNonce().Should().NotBeNullOrEmpty();
        BaselinePolicy.AppendVary("Accept-Encoding, accept-encoding", "Cookie").Should().Be("Accept-Encoding, Cookie");
        BaselinePolicy.AppendVary(null, "Accept-Encoding").Should().Be("Accept-Encoding");
        BaselinePolicy.AppendVary(" , Accept", "Accept").Should().Be("Accept");
        var vary = () => BaselinePolicy.AppendVary(null, "bad token");
        vary.Should().Throw<ArgumentException>().WithMessage("*invalid Vary token*");

        BaselinePolicy.ClearSiteDataHeader(["cookies", "storage"]).Should().Be("\"cookies\", \"storage\"");
        var clear = () => BaselinePolicy.ClearSiteDataHeader([]);
        clear.Should().Throw<ArgumentException>().WithMessage("*non-empty*");
        clear = () => BaselinePolicy.ClearSiteDataHeader(["nope"]);
        clear.Should().Throw<ArgumentException>().WithMessage("*invalid Clear-Site-Data*");

        var preload = () => BaselinePolicy.BuildPreloadLinkHeader([]);
        preload.Should().Throw<ArgumentException>().WithMessage("*non-empty*");
        preload = () => BaselinePolicy.BuildPreloadLinkHeader([new PreloadLink("https://cdn.example/a", "style")]);
        preload.Should().Throw<ArgumentException>().WithMessage("*same-origin path*");
        preload = () => BaselinePolicy.BuildPreloadLinkHeader([new PreloadLink("/app.css", "stylesheet")]);
        preload.Should().Throw<ArgumentException>().WithMessage("*unsupported preload as*");
        preload = () => BaselinePolicy.BuildPreloadLinkHeader([new PreloadLink("/app.css", "style", "not a type")]);
        preload.Should().Throw<ArgumentException>().WithMessage("*invalid preload type*");
        BaselinePolicy.BuildPreloadLinkHeader([new PreloadLink("/app.css", "style", "text/css", Crossorigin: true)])
            .Should().Contain("crossorigin");
        var protocolRelative = () => BaselinePolicy.BuildPreloadLinkHeader([new PreloadLink("//evil", "style")]);
        protocolRelative.Should().Throw<ArgumentException>().WithMessage("*same-origin path*");
    }
}
