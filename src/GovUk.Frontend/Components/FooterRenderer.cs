using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class FooterRenderer : IComponentRenderer
{
    private const string LicenceLogo =
        "<svg\n            aria-hidden=\"true\"\n            focusable=\"false\"\n            class=\"govuk-footer__licence-logo\"\n            xmlns=\"http://www.w3.org/2000/svg\"\n            viewBox=\"0 0 483.2 195.7\"\n            height=\"17\"\n            width=\"41\"\n          >\n            <path\n              fill=\"currentColor\"\n              d=\"M421.5 142.8V.1l-50.7 32.3v161.1h112.4v-50.7zm-122.3-9.6A47.12 47.12 0 0 1 221 97.8c0-26 21.1-47.1 47.1-47.1 16.7 0 31.4 8.7 39.7 21.8l42.7-27.2A97.63 97.63 0 0 0 268.1 0c-36.5 0-68.3 20.1-85.1 49.7A98 98 0 0 0 97.8 0C43.9 0 0 43.9 0 97.8s43.9 97.8 97.8 97.8c36.5 0 68.3-20.1 85.1-49.7a97.76 97.76 0 0 0 149.6 25.4l19.4 22.2h3v-87.8h-80l24.3 27.5zM97.8 145c-26 0-47.1-21.1-47.1-47.1s21.1-47.1 47.1-47.1 47.2 21 47.2 47S123.8 145 97.8 145\"\n            />\n          </svg>";

    public string Name => "footer";

    public string Render(ParamBag parameters)
    {
        var classNames = "govuk-footer";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var containerClass = "govuk-width-container";
        if (parameters.Truthy("containerClasses"))
        {
            containerClass += " " + parameters.Print("containerClasses");
        }

        var builder = new StringBuilder();
        builder.Append("<div class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(">\n  <div class=\"")
            .Append(Nj.Escape(containerClass))
            .Append("\">\n  ")
            .Append(GovUkLogo.FooterCrown)
            .Append("\n\n");

        if (parameters.Length("navigation") > 0)
        {
            builder.Append("      <div class=\"govuk-footer__navigation\">\n");
            foreach (var nav in parameters.Items("navigation"))
            {
                builder.Append(RenderNavigationSection(nav));
            }

            builder.Append("      </div>\n      <hr class=\"govuk-footer__section-break\">\n");
        }

        builder.Append("    <div class=\"govuk-footer__meta\">\n      <div class=\"govuk-footer__meta-item govuk-footer__meta-item--grow\">\n");
        if (parameters.Truthy("meta"))
        {
            builder.Append(RenderMeta(parameters.Get("meta")));
        }

        if (!parameters.Has("contentLicence") || parameters.Truthy("contentLicence"))
        {
            builder.Append(RenderContentLicence(parameters.Get("contentLicence")));
        }

        builder.Append("      </div>\n      <div class=\"govuk-footer__meta-item\">\n        ")
            .Append(RenderCopyright(parameters.Get("copyright")))
            .Append("\n      </div>\n    </div>\n  </div>\n</div>");
        return builder.ToString();
    }

    private static string RenderNavigationSection(ParamBag nav)
    {
        var width = nav.Default("width", "full", whenFalsy: true);
        var builder = new StringBuilder();
        builder.Append("          <div class=\"govuk-footer__section govuk-grid-column-")
        .Append(Nj.Escape(width))
        .Append("\">\n            <h2 class=\"govuk-footer__heading govuk-heading-m\">")
        .Append(Nj.Escape(nav.Print("title")))
        .Append("</h2>\n");

        if (nav.Length("items") > 0)
        {
            var listClass = "govuk-footer__list";
            if (nav.Truthy("columns"))
            {
                listClass += " govuk-footer__list--columns-" + nav.Print("columns");
            }

            builder.Append("              <ul class=\"").Append(Nj.Escape(listClass)).Append("\">\n");
            foreach (var item in nav.Items("items"))
            {
                if (item.Truthy("href") && item.Truthy("text"))
                {
                    builder.Append("                    <li class=\"govuk-footer__list-item\">\n                      <a class=\"govuk-footer__link\" href=\"")
                        .Append(item.Escaped("href"))
                        .Append('"')
                        .Append(item.Attributes())
                        .Append(">\n                        ")
                        .Append(Nj.Escape(item.Print("text")))
                        .Append("\n                      </a>\n                    </li>\n");
                }
            }

            builder.Append("              </ul>\n");
        }

        builder.Append("          </div>\n");
        return builder.ToString();
    }

    private static string RenderMeta(ParamBag meta)
    {
        var builder = new StringBuilder();
        var hiddenTitle = meta.Default("visuallyHiddenTitle", "Support links", whenFalsy: true);
        builder.Append("        <h2 class=\"govuk-visually-hidden\">")
            .Append(Nj.Escape(hiddenTitle))
            .Append("</h2>\n");

        if (meta.Length("items") > 0)
        {
            builder.Append("        <ul class=\"govuk-footer__inline-list\">\n");
            foreach (var item in meta.Items("items"))
            {
                builder.Append("          <li class=\"govuk-footer__inline-list-item\">\n            <a class=\"govuk-footer__link\" href=\"")
                    .Append(item.Escaped("href"))
                    .Append('"')
                    .Append(item.Attributes())
                    .Append(">\n              ")
                    .Append(Nj.Escape(item.Print("text")))
                    .Append("\n            </a>\n          </li>\n");
            }

            builder.Append("        </ul>\n");
        }

        if (meta.Truthy("html") || meta.Truthy("text"))
        {
            builder.Append("        <div class=\"govuk-footer__meta-custom\">\n          ");
            if (meta.Truthy("html"))
            {
                builder.Append(Nj.Indent((meta.Print("html") ?? "").Trim(), 10));
            }
            else
            {
                builder.Append(Nj.Escape(meta.Print("text")));
            }

            builder.Append("\n        </div>\n");
        }

        return builder.ToString();
    }

    private static string RenderContentLicence(ParamBag contentLicence)
    {
        var builder = new StringBuilder();
        builder.Append("          ").Append(LicenceLogo).Append("\n          <span class=\"govuk-footer__licence-description\">\n");
        if (contentLicence.Truthy("html") || contentLicence.Truthy("text"))
        {
            builder.Append("            ");
            if (contentLicence.Truthy("html"))
            {
                builder.Append(Nj.Indent((contentLicence.Print("html") ?? "").Trim(), 12));
            }
            else
            {
                builder.Append(Nj.Escape(contentLicence.Print("text")));
            }

            builder.Append("\n");
        }
        else
        {
            builder.Append("            All content is available under the\n            <a\n              class=\"govuk-footer__link\"\n              href=\"https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/\"\n              rel=\"license\"\n            >Open Government Licence v3.0</a>, except where otherwise stated\n");
        }

        builder.Append("          </span>\n");
        return builder.ToString();
    }

    private static string RenderCopyright(ParamBag copyright)
    {
        const string href =
            "https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/";
        if (copyright.Truthy("html") || copyright.Truthy("text"))
        {
            var body = copyright.Truthy("html")
                ? Nj.Indent((copyright.Print("html") ?? "").Trim(), 10)
                : Nj.Escape(copyright.Print("text"));
            return $"<a\n          class=\"govuk-footer__link govuk-footer__copyright-logo\"\n          href=\"{href}\"\n        >\n          {body}\n        </a>";
        }

        return $"<a\n          class=\"govuk-footer__link govuk-footer__copyright-logo\"\n          href=\"{href}\"\n        >\n          © Crown copyright\n        </a>";
    }
}
