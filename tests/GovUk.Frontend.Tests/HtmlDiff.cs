using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace GovUk.Frontend.Tests;

public static class HtmlDiff
{
    public static string Describe(string expected, string actual)
    {
        var builder = new StringBuilder();
        builder.AppendLine(StringWindow(expected, actual));
        builder.Append(DomDiff(expected, actual));
        return builder.ToString();
    }

    public static string StringWindow(string expected, string actual)
    {
        var limit = Math.Min(expected.Length, actual.Length);
        var index = 0;
        while (index < limit && expected[index] == actual[index])
        {
            index++;
        }

        var start = Math.Max(0, index - 40);
        return "expected: "
            + Quote(expected, start)
            + "\nactual:   "
            + Quote(actual, start);
    }

    public static string DomDiff(string expected, string actual)
    {
        var parser = new HtmlParser();
        var expectedDocument = parser.ParseDocument("<div id=\"expected\">" + expected + "</div>");
        var actualDocument = parser.ParseDocument("<div id=\"actual\">" + actual + "</div>");
        var expectedNodes = expectedDocument.QuerySelector("#expected")?.ChildNodes;
        var actualNodes = actualDocument.QuerySelector("#actual")?.ChildNodes;
        if (expectedNodes is null || actualNodes is null)
        {
            return "DOM: could not parse";
        }

        var notes = new List<string>();
        CompareNodes(expectedNodes, actualNodes, notes, "root");
        return notes.Count == 0
            ? "DOM: equivalent (serialisation still differs)"
            : "DOM:\n" + string.Join('\n', notes);
    }

    private static void CompareNodes(
        INodeList expected,
        INodeList actual,
        List<string> notes,
        string path)
    {
        var count = Math.Max(expected.Length, actual.Length);
        for (var index = 0; index < count; index++)
        {
            if (index >= expected.Length)
            {
                notes.Add($"{path}[{index}] unexpected {Describe(actual[index])}");
                continue;
            }

            if (index >= actual.Length)
            {
                notes.Add($"{path}[{index}] missing {Describe(expected[index])}");
                continue;
            }

            CompareNode(expected[index], actual[index], notes, $"{path}[{index}]");
        }
    }

    private static void CompareNode(INode expected, INode actual, List<string> notes, string path)
    {
        if (expected.NodeType != actual.NodeType)
        {
            notes.Add($"{path} type {expected.NodeType} != {actual.NodeType}");
            return;
        }

        if (expected is IText expectedText && actual is IText actualText)
        {
            if (expectedText.Data != actualText.Data)
            {
                notes.Add($"{path} text {Quote(expectedText.Data)} != {Quote(actualText.Data)}");
            }

            return;
        }

        if (expected is not IElement expectedElement || actual is not IElement actualElement)
        {
            return;
        }

        if (!expectedElement.LocalName.Equals(actualElement.LocalName, StringComparison.Ordinal))
        {
            notes.Add($"{path} <{expectedElement.LocalName}> != <{actualElement.LocalName}>");
            return;
        }

        var expectedNames = expectedElement.Attributes.Select(attribute => attribute.Name).ToList();
        var actualNames = actualElement.Attributes.Select(attribute => attribute.Name).ToList();
        if (!expectedNames.SequenceEqual(actualNames))
        {
            notes.Add(
                $"{path} attributes [{string.Join(' ', expectedNames)}] != [{string.Join(' ', actualNames)}]");
        }

        foreach (var name in expectedNames.Intersect(actualNames, StringComparer.Ordinal))
        {
            var expectedValue = expectedElement.GetAttribute(name);
            var actualValue = actualElement.GetAttribute(name);
            if (expectedValue != actualValue)
            {
                notes.Add($"{path} @{name} {Quote(expectedValue)} != {Quote(actualValue)}");
            }
        }

        CompareNodes(expectedElement.ChildNodes, actualElement.ChildNodes, notes, path + "/" + expectedElement.LocalName);
    }

    private static string Describe(INode node) => node is IElement element
        ? $"<{element.LocalName}>"
        : node.NodeType.ToString();

    private static string Quote(string? value, int start = 0)
    {
        if (value is null)
        {
            return "<null>";
        }

        var slice = value.Length <= start ? "" : value[start..Math.Min(value.Length, start + 120)];
        return System.Text.Json.JsonSerializer.Serialize(slice);
    }
}
