namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides HTML sanitization functionality to prevent XSS attacks.
/// </summary>
public interface IHtmlSanitizer
{
    /// <summary>
    ///     Sanitizes HTML
    /// </summary>
    /// <param name="html">HTML to be sanitized</param>
    /// <returns>Sanitized HTML</returns>
    string Sanitize(string html);
}
