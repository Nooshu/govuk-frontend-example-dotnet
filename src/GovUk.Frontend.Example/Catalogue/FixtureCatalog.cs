using System.Text.Json;

namespace GovUk.Frontend.Example.Catalogue;

public sealed record FixtureExample(string Name, JsonElement Options, string Html, bool Hidden);

public sealed class FixtureCatalog
{
    private readonly Dictionary<string, IReadOnlyList<FixtureExample>> _components;

    public FixtureCatalog(string componentsDirectory)
    {
        var components = new Dictionary<string, IReadOnlyList<FixtureExample>>(StringComparer.Ordinal);
        foreach (var directory in Directory.GetDirectories(componentsDirectory).Order(StringComparer.Ordinal))
        {
            var path = Path.Combine(directory, "fixtures.json");
            if (!File.Exists(path))
            {
                continue;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var name = document.RootElement.GetProperty("component").GetString()
                ?? Path.GetFileName(directory);
            var fixtures = new List<FixtureExample>();
            foreach (var fixture in document.RootElement.GetProperty("fixtures").EnumerateArray())
            {
                fixtures.Add(new FixtureExample(
                    fixture.GetProperty("name").GetString() ?? "",
                    fixture.GetProperty("options").Clone(),
                    fixture.GetProperty("html").GetString() ?? "",
                    fixture.TryGetProperty("hidden", out var hidden) && hidden.ValueKind == JsonValueKind.True));
            }

            components.Add(name, fixtures);
        }

        _components = components;
    }

    public IReadOnlyList<string> Names => _components.Keys.Order(StringComparer.Ordinal).ToArray();

    public bool TryGet(string name, out IReadOnlyList<FixtureExample> fixtures) =>
        _components.TryGetValue(name, out fixtures!);

    public FixtureExample? Find(string component, string? fixtureName)
    {
        if (!_components.TryGetValue(component, out var fixtures) || fixtures.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrEmpty(fixtureName))
        {
            return fixtures.FirstOrDefault(fixture => !fixture.Hidden) ?? fixtures[0];
        }

        return fixtures.FirstOrDefault(fixture => fixture.Name == fixtureName);
    }
}
