using System.Reflection;
using System.Text.Json;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public static class ComponentCatalog
{
    private static readonly IReadOnlyDictionary<string, IComponentRenderer> Renderers = Discover();

    public static IEnumerable<string> Names => Renderers.Keys;

    public static string Render(string name, JsonElement options) =>
        Render(name, ParamBag.FromJson(options));

    public static string Render(string name, ParamBag parameters)
    {
        if (!Renderers.TryGetValue(name, out var renderer))
        {
            throw new ArgumentException($"Unknown GOV.UK component '{name}'.", nameof(name));
        }

        return renderer.Render(parameters);
    }

    private static Dictionary<string, IComponentRenderer> Discover()
    {
        var renderers = new Dictionary<string, IComponentRenderer>(StringComparer.Ordinal);
        foreach (var type in typeof(ComponentCatalog).Assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IComponentRenderer).IsAssignableFrom(type))
            {
                continue;
            }

            var renderer = (IComponentRenderer)Activator.CreateInstance(type)!;
            renderers.Add(renderer.Name, renderer);
        }

        return renderers;
    }
}
