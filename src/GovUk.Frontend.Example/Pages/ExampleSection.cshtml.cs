using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GovUk.Frontend.Example.Pages;

public class ExampleSectionModel : PageModel
{
    public static readonly string[] Routes =
    [
        "help",
        "fees",
        "updates",
        "guidance",
        "cookies",
        "accessibility",
        "about",
    ];

    private static readonly Dictionary<string, string> Titles = new(StringComparer.Ordinal)
    {
        ["help"] = "Help",
        ["fees"] = "Licence fees",
        ["updates"] = "Service updates",
        ["guidance"] = "Guidance",
        ["cookies"] = "Cookies",
        ["accessibility"] = "Accessibility",
        ["about"] = "About this example",
    };

    public string Heading { get; private set; } = "";

    public void OnGet()
    {
        var key = HttpContext.Request.Path.ToString().Trim('/');
        Heading = Titles.TryGetValue(key, out var heading) ? heading : "This example";
    }
}
