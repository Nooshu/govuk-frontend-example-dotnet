using Microsoft.AspNetCore.Http;

namespace GovUk.Frontend.Example.Hosting;

public static class ServiceHome
{
    public const string StartPath = "/apply";

    public const string CataloguePath = "/components";

    public static bool RedirectsRootToStart(string method, PathString path) =>
        IsRead(method) && path == "/";

    private static bool IsRead(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
}
