namespace Umbraco.Cms.Core.Security;

/// <inheritdoc />
public class NoopMarkdownSanitizer : IMarkdownSanitizer
{
    /// <inheritdoc />
    public string Sanitize(string markdown) => markdown;
}
