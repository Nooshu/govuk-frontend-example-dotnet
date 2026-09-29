using System.Text.Json;
using GovUk.Frontend.Components;

namespace GovUk.Frontend.Tests;

public static class FixtureSet
{
    public static string Root { get; } = FindRoot();

    public static IReadOnlyList<ComponentFixture> All { get; } = Load();

    public static IEnumerable<object[]> Cases()
    {
        var only = Environment.GetEnvironmentVariable("GOVUK_COMPONENT");
        foreach (var fixture in All)
        {
            if (!string.IsNullOrEmpty(only)
                && !string.Equals(fixture.Component, only, StringComparison.Ordinal))
            {
                continue;
            }

            yield return [fixture.Component, fixture.Name];
        }
    }

    public static ComponentFixture Get(string component, string name) =>
        All.Single(fixture => fixture.Component == component && fixture.Name == name);

    private static List<ComponentFixture> Load()
    {
        var directory = Path.Combine(
            Root,
            "node_modules",
            "govuk-frontend",
            "dist",
            "govuk",
            "components");
        var fixtures = new List<ComponentFixture>();
        foreach (var componentDirectory in Directory.GetDirectories(directory).Order(StringComparer.Ordinal))
        {
            var path = Path.Combine(componentDirectory, "fixtures.json");
            if (!File.Exists(path))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var component = document.RootElement.GetProperty("component").GetString()
                ?? Path.GetFileName(componentDirectory);
            foreach (var fixture in document.RootElement.GetProperty("fixtures").EnumerateArray())
            {
                fixtures.Add(new ComponentFixture(
                    component,
                    fixture.GetProperty("name").GetString() ?? "",
                    fixture.GetProperty("options").Clone(),
                    fixture.GetProperty("html").GetString() ?? "",
                    fixture.TryGetProperty("hidden", out var hidden) && hidden.ValueKind == JsonValueKind.True));
            }
        }

        return fixtures;
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "package.json")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find the repository root.");
    }
}

public sealed record ComponentFixture(
    string Component,
    string Name,
    JsonElement Options,
    string Html,
    bool Hidden);
