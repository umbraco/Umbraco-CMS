namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Sanitizer service for the markdown editor.
/// </summary>
public interface IMarkdownSanitizer
{
    /// <summary>
    ///     Sanitizes Markdown
    /// </summary>
    /// <param name="markdown">Markdown to be sanitized</param>
    /// <returns>Sanitized Markdown</returns>
    string Sanitize(string markdown);
}
