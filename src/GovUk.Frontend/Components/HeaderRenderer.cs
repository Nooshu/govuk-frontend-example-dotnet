using System.Text.Json.Nodes;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class HeaderRenderer : IComponentRenderer
{
    public string Name => "header";

    public string Render(ParamBag parameters)
    {
        var logoHtml = Nj.Indent("  " + GovUkLogo.HeaderLogotype, 8);
        if (parameters.Truthy("productName"))
        {
            logoHtml += $"\n        <span class=\"govuk-header__product-name\">{parameters.Print("productName")}</span>";
        }
        else
        {
            logoHtml += "\n        ";
        }

        var genericOptions = new JsonObject
        {
            ["_namespace"] = "govuk",
            ["logoHtml"] = logoHtml,
            ["url"] = parameters.Default("homepageUrl", "//gov.uk", whenFalsy: true),
        };

        if (parameters.Has("containerClasses"))
        {
            genericOptions["containerClasses"] = parameters.Print("containerClasses");
        }

        if (parameters.Has("classes"))
        {
            genericOptions["classes"] = parameters.Print("classes");
        }

        if (parameters.Has("attributes"))
        {
            genericOptions["attributes"] = JsonNode.Parse(parameters.Get("attributes").Scalar() ?? "{}");
        }

        var genericBag = ParamBag.FromJson(PhaseBannerRenderer.JsonNodeToElement(genericOptions));
        return GenericHeaderRenderer.RenderGenericHeader(genericBag);
    }
}
