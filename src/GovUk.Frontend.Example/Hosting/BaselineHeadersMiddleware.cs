using GovUk.Frontend.Baseline;

namespace GovUk.Frontend.Example.Hosting;

public sealed class BaselineHeadersMiddleware(RequestDelegate next, BaselinePolicy policy)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            Apply(context);
            return Task.CompletedTask;
        });
        await next(context);
    }

    private void Apply(HttpContext context)
    {
        var kind = ResolveKind(context);
        var document = kind is "document" or "sensitive-document";
        var setsCookie = document && context.Response.Headers.ContainsKey("Set-Cookie");
        var secure = context.Request.IsHttps
            || string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase);
        var contentType = context.Response.ContentType;
        var result = policy.BuildResponseHeaders(new ResponseHeaderOptions
        {
            Kind = kind,
            SecureTransport = secure,
            SetsCookie = setsCookie,
            ContentType = string.IsNullOrEmpty(contentType) ? null : contentType,
        });

        foreach (var name in result.Remove)
        {
            context.Response.Headers.Remove(name);
        }

        foreach (var (name, value) in result.Headers)
        {
            context.Response.Headers[name] = value;
        }

        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }

    private static string ResolveKind(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/assets"))
        {
            return "fingerprinted-asset";
        }

        var marked = context.GetEndpoint()?.Metadata.GetMetadata<ResponseCacheKindAttribute>();
        return marked?.Kind ?? "document";
    }
}
