namespace GovUk.Frontend.Example.Catalogue;

public static class ComponentInfo
{
    private const string DesignSystem = "https://design-system.service.gov.uk/components";

    private static readonly Dictionary<string, (string Title, string Url)> Known = new(StringComparer.Ordinal)
    {
        ["accordion"] = ("Accordion", $"{DesignSystem}/accordion/"),
        ["back-link"] = ("Back link", $"{DesignSystem}/back-link/"),
        ["breadcrumbs"] = ("Breadcrumbs", $"{DesignSystem}/breadcrumbs/"),
        ["button"] = ("Button", $"{DesignSystem}/button/"),
        ["character-count"] = ("Character count", $"{DesignSystem}/character-count/"),
        ["checkboxes"] = ("Checkboxes", $"{DesignSystem}/checkboxes/"),
        ["cookie-banner"] = ("Cookie banner", $"{DesignSystem}/cookie-banner/"),
        ["date-input"] = ("Date input", $"{DesignSystem}/date-input/"),
        ["details"] = ("Details", $"{DesignSystem}/details/"),
        ["error-message"] = ("Error message", $"{DesignSystem}/error-message/"),
        ["error-summary"] = ("Error summary", $"{DesignSystem}/error-summary/"),
        ["exit-this-page"] = ("Exit this page", $"{DesignSystem}/exit-this-page/"),
        ["feedback"] = ("Feedback", $"{DesignSystem}/feedback/"),
        ["fieldset"] = ("Fieldset", $"{DesignSystem}/fieldset/"),
        ["file-upload"] = ("File upload", $"{DesignSystem}/file-upload/"),
        ["footer"] = ("Footer", $"{DesignSystem}/footer/"),
        ["generic-header"] = ("Generic header", "https://design-system.service.gov.uk/styles/page-template/"),
        ["header"] = ("Header", $"{DesignSystem}/header/"),
        ["hint"] = ("Hint", "https://design-system.service.gov.uk/get-started/labels-legends-headings/"),
        ["input"] = ("Text input", $"{DesignSystem}/text-input/"),
        ["inset-text"] = ("Inset text", $"{DesignSystem}/inset-text/"),
        ["label"] = ("Label", "https://design-system.service.gov.uk/get-started/labels-legends-headings/"),
        ["language-navigation"] = ("Language navigation", $"{DesignSystem}/language-navigation/"),
        ["notification-banner"] = ("Notification banner", $"{DesignSystem}/notification-banner/"),
        ["pagination"] = ("Pagination", $"{DesignSystem}/pagination/"),
        ["panel"] = ("Panel", $"{DesignSystem}/panel/"),
        ["password-input"] = ("Password input", $"{DesignSystem}/password-input/"),
        ["phase-banner"] = ("Phase banner", $"{DesignSystem}/phase-banner/"),
        ["radios"] = ("Radios", $"{DesignSystem}/radios/"),
        ["select"] = ("Select", $"{DesignSystem}/select/"),
        ["service-navigation"] = ("Service navigation", $"{DesignSystem}/service-navigation/"),
        ["skip-link"] = ("Skip link", $"{DesignSystem}/skip-link/"),
        ["summary-list"] = ("Summary list", $"{DesignSystem}/summary-list/"),
        ["table"] = ("Table", $"{DesignSystem}/table/"),
        ["tabs"] = ("Tabs", $"{DesignSystem}/tabs/"),
        ["tag"] = ("Tag", $"{DesignSystem}/tag/"),
        ["task-list"] = ("Task list", $"{DesignSystem}/task-list/"),
        ["textarea"] = ("Textarea", $"{DesignSystem}/textarea/"),
        ["warning-text"] = ("Warning text", $"{DesignSystem}/warning-text/"),
    };

    public static string Title(string name) =>
        Known.TryGetValue(name, out var info) ? info.Title : TitleFromKebab(name);

    public static string DesignSystemUrl(string name) =>
        Known.TryGetValue(name, out var info) ? info.Url : $"{DesignSystem}/{name}/";

    private static string TitleFromKebab(string name)
    {
        var words = name.Split('-', StringSplitOptions.RemoveEmptyEntries);
        for (var index = 0; index < words.Length; index++)
        {
            var word = words[index];
            words[index] = char.ToUpperInvariant(word[0]) + word[1..];
        }

        return string.Join(' ', words);
    }
}
