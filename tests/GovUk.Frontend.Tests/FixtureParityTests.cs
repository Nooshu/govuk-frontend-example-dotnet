using AngleSharp.Html.Parser;
using GovUk.Frontend.Components;

namespace GovUk.Frontend.Tests;

public class FixtureParityTests
{
    [Theory]
    [MemberData(nameof(FixtureSet.Cases), MemberType = typeof(FixtureSet))]
    public void Backend_html_matches_the_official_fixture(string component, string name)
    {
        var fixture = FixtureSet.Get(component, name);
        var actual = ComponentCatalog.Render(component, fixture.Options);
        if (actual == fixture.Html)
        {
            return;
        }

        var diff = HtmlDiff.Describe(fixture.Html, actual);
        Assert.Fail($"{component} / {name}\n{diff}");
    }

    [Fact]
    public void Every_fixture_component_has_a_renderer()
    {
        var expected = FixtureSet.All.Select(fixture => fixture.Component).Distinct().Order(StringComparer.Ordinal);
        var actual = ComponentCatalog.Names.Order(StringComparer.Ordinal);
        Assert.Equal(expected, actual);
    }
}
