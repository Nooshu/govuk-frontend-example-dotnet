using System.Text.Json;

namespace GovUk.Frontend.Example.Hosting;

public sealed record AssetManifest(string Stylesheet, string Script, string AssetPrefix)
{
    public static AssetManifest Load(string webRoot)
    {
        var path = Path.Combine(webRoot, "asset-manifest.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        return new AssetManifest(
            root.GetProperty("stylesheet").GetString() ?? "",
            root.GetProperty("script").GetString() ?? "",
            root.GetProperty("assetPrefix").GetString() ?? "");
    }
}
