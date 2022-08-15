namespace Umbraco.Cms.Core.Strings;

/// <summary>
///     Represents an HTML-encoded string that should not be encoded again.
/// </summary>
public interface IHtmlEncodedString
{
    /// <summary>
    ///     Returns an HTML-encoded string.
    /// </summary>
    /// <returns>An HTML-encoded string.</returns>
    string? ToHtmlString();
}
