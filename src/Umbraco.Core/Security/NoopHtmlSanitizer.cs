namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A no-operation implementation of <see cref="IHtmlSanitizer" /> that returns HTML unchanged.
/// </summary>
/// <remarks>
///     This implementation does not perform any sanitization and should only be used when HTML
///     sanitization is not required or is handled elsewhere.
/// </remarks>
public class NoopHtmlSanitizer : IHtmlSanitizer
{
    /// <inheritdoc />
    public string Sanitize(string html) => html;
}
