using FluentAssertions;
using GovUk.Frontend.Tests;

namespace GovUk.Frontend.Tests;

public class HtmlDiffTests
{
    [Fact]
    public void Describes_a_string_and_dom_mismatch()
    {
        var diff = HtmlDiff.Describe("<p class=\"a\">One</p>", "<p class=\"b\">Two</p>");
        diff.Should().Contain("expected:");
        diff.Should().Contain("@class");
    }

    [Fact]
    public void Reports_equivalent_dom_when_only_serialisation_differs()
    {
        var diff = HtmlDiff.DomDiff("<p>Hi</p>", "<p>Hi</p>");
        diff.Should().Contain("equivalent");
    }

    [Fact]
    public void Reports_missing_and_unexpected_nodes()
    {
        var diff = HtmlDiff.DomDiff("<p>Hi</p><span></span>", "<p>Hi</p>");
        diff.Should().Contain("missing");
        var extra = HtmlDiff.DomDiff("<p>Hi</p>", "<p>Hi</p><em></em>");
        extra.Should().Contain("unexpected");
    }
}
