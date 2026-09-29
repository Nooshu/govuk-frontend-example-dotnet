using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class GenericHeaderRenderer : IComponentRenderer
{
    public string Name => "generic-header";

    public string Render(ParamBag parameters) => RenderGenericHeader(parameters);

    internal static string RenderGenericHeader(ParamBag parameters)
    {
        var ns = parameters.Has("_namespace") ? parameters.Print("_namespace")! : "govuk-generic";
        var headerClass = $"{ns}-header";
        if (parameters.Truthy("classes"))
        {
            headerClass += " " + parameters.Print("classes");
        }

        var containerClasses = parameters.Default("containerClasses", "govuk-width-container", whenFalsy: true);
        var url = parameters.Default("url", "/", whenFalsy: true);
        var logo = parameters.Truthy("logoHtml")
            ? parameters.Print("logoHtml") ?? ""
            : Nj.Escape(parameters.Print("logoText"));

        var builder = new StringBuilder();
        builder.Append("<div class=\"").Append(Nj.Escape(headerClass)).Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  <div class=\"")
            .Append(Nj.Escape($"{ns}-header__container {containerClasses}"))
            .Append("\">\n    <div class=\"")
            .Append(Nj.Escape($"{ns}-header__logo"))
            .Append("\">\n      <a href=\"")
            .Append(Nj.Escape(url))
            .Append("\" class=\"")
            .Append(Nj.Escape($"{ns}-header__homepage-link"))
            .Append("\">\n        ")
            .Append(logo)
            .Append("\n      </a>\n    </div>\n  </div>\n</div>");
        return builder.ToString();
    }
}
