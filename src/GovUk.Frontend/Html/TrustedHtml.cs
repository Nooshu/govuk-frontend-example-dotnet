namespace GovUk.Frontend.Html;

/// <summary>
/// HTML that is allowed to be written without encoding.
/// Plain text must stay a <see cref="string"/> so it is escaped.
/// </summary>
public readonly record struct TrustedHtml
{
    private TrustedHtml(string value) => Value = value;

    public string Value { get; }

    public static TrustedHtml FromTrusted(string? html) => new(html ?? "");

    public override string ToString() => Value;
}
