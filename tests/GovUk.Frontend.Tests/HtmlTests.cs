using System.Text.Json;
using FluentAssertions;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Tests;

public class NjTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("plain", "plain")]
    [InlineData("a&b<c>d\"e'f", "a&amp;b&lt;c&gt;d&quot;e&#39;f")]
    [InlineData("line\nbreak", "line\nbreak")]
    public void Escape_matches_nunjucks(string? input, string expected) =>
        Nj.Escape(input).Should().Be(expected);

    [Fact]
    public void Indent_skips_the_first_line_unless_asked()
    {
        Nj.Indent("a\nb", 2).Should().Be("a\n  b");
        Nj.Indent("a\nb", 2, first: true).Should().Be("  a\n  b");
        Nj.Indent("", 2).Should().Be("");
        Nj.Indent(null, 4).Should().Be("");
    }

    [Fact]
    public void Trim_removes_surrounding_whitespace() =>
        Nj.Trim(" \n x \n").Should().Be("x");
}

public class ParamBagTests
{
    [Fact]
    public void Attributes_preserve_order_types_and_optional_booleans()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "attributes": { "aria-hidden": "true", "data-n": 2 },
              "spellcheck": true,
              "rows": 0,
              "empty": "",
              "flag": false,
              "missingDefault": null,
              "items": [null, { "text": "A" }]
            }
            """);
        var parameters = ParamBag.FromJson(document.RootElement);

        parameters.Attributes().Should().Be(" aria-hidden=\"true\" data-n=\"2\"");
        parameters.Truthy("spellcheck").Should().BeTrue();
        parameters.Truthy("rows").Should().BeFalse();
        parameters.Truthy("empty").Should().BeFalse();
        parameters.Default("type", "text", whenFalsy: true).Should().Be("text");
        parameters.Default("empty", "fallback").Should().Be("");
        parameters.Default("absent", "fallback").Should().Be("fallback");
        parameters.Default("flag", "fallback", whenFalsy: true).Should().Be("fallback");
        parameters.Print("missingDefault").Should().BeNull();
        parameters.Defined("missingDefault").Should().BeTrue();
        parameters.IsExactlyFalse("flag").Should().BeTrue();
        parameters.IsExactlyTrue("spellcheck").Should().BeTrue();
        parameters.Items("items").Should().HaveCount(2);
        parameters.Items("items")[0].IsTruthy.Should().BeFalse();
        parameters.Items("items")[1].Print("text").Should().Be("A");
        parameters.Length("items").Should().Be(2);
        parameters.Length("absent").Should().Be(0);
        parameters.Get("nope").IsUndefined.Should().BeTrue();
        parameters.Escaped("empty").Should().Be("");
        parameters.Pairs().Select(pair => pair.Name).Should().Contain("attributes");
        parameters.Get("items").Pairs().Should().BeEmpty();
    }

    [Fact]
    public void Optional_attribute_objects_match_govukAttributes()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "hidden": { "value": true, "optional": true },
              "skip": { "value": false, "optional": true },
              "name": { "value": null, "optional": false }
            }
            """);
        ParamBag.RenderAttributes(document.RootElement).Should().Be(" hidden name=\"\"");
    }

    [Fact]
    public void String_attributes_are_passed_through() =>
        ParamBag.RenderAttributes(JsonDocument.Parse("\" data-x\"").RootElement).Should().Be(" data-x");

    [Fact]
    public void Undefined_bag_prints_nothing()
    {
        ParamBag.Undefined.Print("x").Should().BeNull();
        ParamBag.Undefined.IsTruthy.Should().BeFalse();
        ParamBag.Undefined.Attributes().Should().Be("");
        ParamBag.Undefined.Scalar().Should().BeNull();
    }
}
