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

    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.Ordinal)
    {
        ["accordion"] = "Lets users show and hide sections of related content.",
        ["back-link"] = "Link to the previous page in a journey.",
        ["breadcrumbs"] = "Helps users move between levels of a section.",
        ["button"] = "Starts or continues an action.",
        ["character-count"] = "Shows how many characters are left in a textarea.",
        ["checkboxes"] = "Lets users select one or more options.",
        ["cookie-banner"] = "Asks users to accept or reject analytics cookies.",
        ["date-input"] = "Asks users for a date they already know.",
        ["details"] = "Hides content that only some users need.",
        ["error-message"] = "Tells users how to fix a field that failed validation.",
        ["error-summary"] = "Summarises form errors at the top of the page.",
        ["exit-this-page"] = "Lets users leave a page quickly. For services where someone may be in danger.",
        ["feedback"] = "Asks users what they think of a page. Trial component in Frontend 6.5.",
        ["fieldset"] = "Groups related form fields, such as an address.",
        ["file-upload"] = "Lets users select a file to upload.",
        ["footer"] = "Page footer with Open Government Licence and Crown copyright.",
        ["generic-header"] = "Header for services that are not branded as GOV.UK. Shown in the catalogue only.",
        ["header"] = "The GOV.UK masthead.",
        ["hint"] = "Extra help for a form field. Form controls include it; the catalogue shows it on its own.",
        ["input"] = "Lets users enter a single line of text.",
        ["inset-text"] = "Draws attention to important content on the page.",
        ["label"] = "Labels a form field. Form controls include it; the catalogue shows it on its own.",
        ["language-navigation"] = "Lets users switch between languages. Trial component in Frontend 6.5.",
        ["notification-banner"] = "Tells users about something that affects the whole service.",
        ["pagination"] = "Splits a long list across pages.",
        ["panel"] = "Confirms a transaction is complete.",
        ["password-input"] = "Lets users enter a password, with a control to show or hide it.",
        ["phase-banner"] = "Shows users that the service is still being tried out.",
        ["radios"] = "Lets users select one option from a list.",
        ["select"] = "Lets users choose one option from a long list.",
        ["service-navigation"] = "Shows the service name under the GOV.UK masthead.",
        ["skip-link"] = "Lets keyboard users skip to the main content.",
        ["summary-list"] = "Summarises answers so users can check them.",
        ["table"] = "Shows information in rows and columns.",
        ["tabs"] = "Lets users switch between related views. Content stays in the page without JavaScript.",
        ["tag"] = "Shows a short status, such as on a task list.",
        ["task-list"] = "Shows the tasks in an application and whether they are done.",
        ["textarea"] = "Lets users enter more than one line of text. This service uses character count, which includes a textarea.",
        ["warning-text"] = "Tells users about something important before they continue.",
    };

    public static string Title(string name) =>
        Known.TryGetValue(name, out var info) ? info.Title : TitleFromKebab(name);

    public static string Description(string name) =>
        Descriptions.TryGetValue(name, out var description) ? description : "A GOV.UK Frontend component.";

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
