using Microsoft.AspNetCore.Hosting;

namespace GovUk.Frontend.Example.Hosting;

public static class ListenAddress
{
    public static void Apply(IWebHostBuilder host)
    {
        if (FromPort(Environment.GetEnvironmentVariable("PORT")) is { } listen)
        {
            host.UseUrls(listen);
        }
    }

    public static string? FromPort(string? port) =>
        string.IsNullOrWhiteSpace(port) ? null : $"http://0.0.0.0:{port.Trim()}";
}
