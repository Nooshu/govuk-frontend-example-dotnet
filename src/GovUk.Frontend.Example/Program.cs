using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using GovUk.Frontend.Baseline;
using GovUk.Frontend.Components;
using GovUk.Frontend.Example.Catalogue;
using GovUk.Frontend.Example.Hosting;
using GovUk.Frontend.Example.Pages;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

ListenAddress.Apply(builder.WebHost);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Services.AddRazorPages(options =>
{
    foreach (var route in ExampleSectionModel.Routes)
    {
        options.Conventions.AddPageRoute("/ExampleSection", route);
    }
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "fishing-example";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddSingleton(_ => BaselinePolicy.Load(RepoPaths.PolicyFile()));
builder.Services.AddSingleton(_ => new FixtureCatalog(RepoPaths.ComponentFixturesDirectory()));
builder.Services.AddSingleton(services =>
{
    var environment = services.GetRequiredService<IWebHostEnvironment>();
    return AssetManifest.Load(environment.WebRootPath);
});

var app = builder.Build();
app.UseForwardedHeaders();
app.UseResponseCompression();
app.UseMiddleware<BaselineHeadersMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();
app.MapGet("/health", () => Results.Text("ok", "text/plain")).WithMetadata(new ResponseCacheKindAttribute("sensitive-document"));
app.MapGet("/robots.txt", () => Results.Text("User-agent: *\nDisallow:\n", "text/plain"));
app.MapGet("/components/{name}/raw", (string name, string? fixture, FixtureCatalog catalog) =>
{
    var example = catalog.Find(name, fixture);
    if (example is null)
    {
        return Results.NotFound();
    }

    return Results.Content(ComponentCatalog.Render(name, example.Options), "text/html");
});
app.Run();

public partial class Program;
