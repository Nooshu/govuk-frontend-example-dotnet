using GovUk.Frontend.Example.Catalogue;
using GovUk.Frontend.Example.Hosting;
using Microsoft.AspNetCore.Builder;

namespace GovUk.Frontend.Example.Tests;

public class HostingTests
{
    [Fact]
    public void Listen_address_uses_port_when_it_is_set()
    {
        Assert.Null(ListenAddress.FromPort(null));
        Assert.Null(ListenAddress.FromPort("  "));
        Assert.Equal("http://0.0.0.0:8080", ListenAddress.FromPort(" 8080 "));

        var previous = Environment.GetEnvironmentVariable("PORT");
        try
        {
            Environment.SetEnvironmentVariable("PORT", null);
            ListenAddress.Apply(WebApplication.CreateBuilder().WebHost);

            Environment.SetEnvironmentVariable("PORT", "9");
            ListenAddress.Apply(WebApplication.CreateBuilder().WebHost);
        }
        finally
        {
            Environment.SetEnvironmentVariable("PORT", previous);
        }
    }

    [Fact]
    public void Asset_manifest_uses_empty_strings_when_values_are_null()
    {
        var root = Directory.CreateTempSubdirectory("govuk-assets-");
        try
        {
            File.WriteAllText(
                Path.Combine(root.FullName, "asset-manifest.json"),
                """{"stylesheet":null,"script":null,"assetPrefix":null}""");
            var manifest = AssetManifest.Load(root.FullName);
            Assert.Equal("", manifest.Stylesheet);
            Assert.Equal("", manifest.Script);
            Assert.Equal("", manifest.AssetPrefix);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void Repo_paths_resolve_fixtures_policy_and_missing_files()
    {
        var root = Directory.CreateTempSubdirectory("govuk-repo-");
        try
        {
            var nested = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "app"));
            Directory.CreateDirectory(Path.Combine(root.FullName, "baseline"));
            File.WriteAllText(Path.Combine(root.FullName, "package.json"), "{}");
            File.WriteAllText(Path.Combine(root.FullName, "baseline", "policy.json"), "{}");

            var found = RepoPaths.Find(["baseline", "policy.json"], [nested.FullName]);
            Assert.EndsWith(Path.Combine("baseline", "policy.json"), found);
            Assert.Null(RepoPaths.Find(["missing.json"], [root.FullName]));
            Assert.Equal(root.FullName, RepoPaths.FindDirectoryContaining("package.json", [nested.FullName]));
            Assert.Null(RepoPaths.FindDirectoryContaining("missing.json", [root.FullName]));

            var outside = Directory.CreateTempSubdirectory("govuk-outside-");
            try
            {
                var roots = RepoPaths.SearchRoots(nested.FullName, outside.FullName).ToArray();
                Assert.Equal([nested.FullName, outside.FullName], roots);
                Assert.Single(RepoPaths.SearchRoots(nested.FullName, root.FullName));
                Assert.Single(RepoPaths.SearchRoots(root.FullName, root.FullName));
            }
            finally
            {
                outside.Delete(recursive: true);
            }

            Assert.EndsWith(
                Path.Combine("dist", "govuk", "components"),
                RepoPaths.FixturesFromRoot(Path.Combine(root.FullName, "frontend"), null));
            Assert.EndsWith(
                Path.Combine("node_modules", "govuk-frontend", "dist", "govuk", "components"),
                RepoPaths.FixturesFromRoot("  ", root.FullName));
            Assert.Throws<InvalidOperationException>(() => RepoPaths.FixturesFromRoot(null, null));
            Assert.Equal("ok", RepoPaths.Require("ok", "anything"));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void Fixture_catalog_skips_directories_without_fixtures_and_defaults_names()
    {
        var root = Directory.CreateTempSubdirectory("govuk-fixtures-");
        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "empty"));
            var component = Directory.CreateDirectory(Path.Combine(root.FullName, "button"));
            File.WriteAllText(
                Path.Combine(component.FullName, "fixtures.json"),
                """
                {
                  "component": null,
                  "fixtures": [
                    {"name": null, "options": {}, "html": null}
                  ]
                }
                """);
            var catalog = new FixtureCatalog(root.FullName);
            Assert.Equal(["button"], catalog.Names);
            var example = catalog.Find("button", null);
            Assert.NotNull(example);
            Assert.Equal("", example.Name);
            Assert.Equal("", example.Html);
            Assert.False(example.Hidden);
            Assert.Null(catalog.Find("button", "other"));
            Assert.Null(catalog.Find("missing", null));

            File.WriteAllText(
                Path.Combine(component.FullName, "fixtures.json"),
                """{"component":"button","fixtures":[]}""");
            var empty = new FixtureCatalog(root.FullName);
            Assert.Null(empty.Find("button", null));

            File.WriteAllText(
                Path.Combine(component.FullName, "fixtures.json"),
                """
                {"component":"button","fixtures":[
                  {"name":"secret","options":{},"html":"","hidden":true},
                  {"name":"shown","options":{},"html":""}
                ]}
                """);
            Assert.Equal("shown", new FixtureCatalog(root.FullName).Find("button", null)!.Name);

            File.WriteAllText(
                Path.Combine(component.FullName, "fixtures.json"),
                """{"component":"button","fixtures":[{"name":"secret","options":{},"html":"","hidden":true}]}""");
            Assert.Equal("secret", new FixtureCatalog(root.FullName).Find("button", null)!.Name);
            Assert.Equal("My Widget", ComponentInfo.Title("my-widget"));
            Assert.Equal("Button", ComponentInfo.Title("button"));
            Assert.Contains("/button/", ComponentInfo.DesignSystemUrl("button"), StringComparison.Ordinal);
            Assert.Contains("/my-widget/", ComponentInfo.DesignSystemUrl("my-widget"), StringComparison.Ordinal);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }
}
