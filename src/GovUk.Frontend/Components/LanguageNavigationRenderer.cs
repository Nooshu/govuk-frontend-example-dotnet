using System.Text;
using GovUk.Frontend.Html;

namespace GovUk.Frontend.Components;

public sealed class LanguageNavigationRenderer : IComponentRenderer
{
    public string Name => "language-navigation";

    public string Render(ParamBag parameters)
    {
        var classNames = "govuk-language-navigation";
        if (parameters.Truthy("classes"))
        {
            classNames += " " + parameters.Print("classes");
        }

        var ariaLabel = parameters.Default("ariaLabel", "Language");
        var builder = new StringBuilder();
        builder.Append("<nav class=\"")
            .Append(Nj.Escape(classNames))
            .Append('"')
            .Append(parameters.Attributes())
            .Append(" aria-label=\"")
            .Append(Nj.Escape(ariaLabel))
            .Append("\">\n  <ul class=\"govuk-language-navigation__list\">\n");

        foreach (var item in parameters.Items("items"))
        {
            var isCurrent = item.Truthy("current") || !item.Truthy("href");
            if (isCurrent)
            {
                builder.Append("    <li class=\"govuk-language-navigation__list-item\">\n      <span class=\"govuk-language-navigation__text");
                if (item.Truthy("classes"))
                {
                    builder.Append(' ').Append(item.Print("classes"));
                }

                builder.Append("\"\n        aria-current=\"true\"");
                AppendLangDir(builder, item);
                builder.Append(item.Attributes())
                    .Append('>')
                    .Append(ItemBody(item))
                    .Append("</span>\n    </li>\n");
            }
            else
            {
                builder.Append("    <li class=\"govuk-language-navigation__list-item\">\n      <a class=\"govuk-language-navigation__link");
                if (item.Truthy("classes"))
                {
                    builder.Append(' ').Append(item.Print("classes"));
                }

                builder.Append("\" href=\"")
                    .Append(item.Escaped("href"))
                    .Append("\" rel=\"alternate\"");
                if (item.Truthy("lang"))
                {
                    builder.Append(" lang=\"").Append(item.Escaped("lang")).Append('"');
                }

                if (item.Truthy("hrefLang") || item.Truthy("lang"))
                {
                    builder.Append(" hreflang=\"")
                        .Append(Nj.Escape(item.Truthy("hrefLang") ? item.Print("hrefLang") : item.Print("lang")))
                        .Append('"');
                }

                if (item.Truthy("dir"))
                {
                    builder.Append(" dir=\"").Append(item.Escaped("dir")).Append('"');
                }

                builder.Append(item.Attributes())
                    .Append('>')
                    .Append(ItemBody(item));
                if (item.Truthy("languageDescriptionText"))
                {
                    builder.Append("<span class=\"govuk-visually-hidden\"> ")
                        .Append(Nj.Escape(item.Print("languageDescriptionText")))
                        .Append("</span>");
                }

                builder.Append("      </a>\n    </li>\n");
            }
        }

        builder.Append("  </ul>\n</nav>");
        return builder.ToString();
    }

    private static void AppendLangDir(StringBuilder builder, ParamBag item)
    {
        if (item.Truthy("lang"))
        {
            builder.Append(" lang=\"").Append(item.Escaped("lang")).Append('"');
        }

        if (item.Truthy("dir"))
        {
            builder.Append(" dir=\"").Append(item.Escaped("dir")).Append('"');
        }
    }

    private static string ItemBody(ParamBag item) =>
        item.Truthy("html") ? item.Print("html") ?? "" : Nj.Escape(item.Print("text"));
}
