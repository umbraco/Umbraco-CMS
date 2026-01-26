namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Defines a service that converts Markdown-formatted text to HTML.
/// </summary>
public interface IMarkdownToHtmlConverter
{
    /// <summary>
    /// Converts the specified Markdown-formatted text to an HTML-encoded string.
    /// </summary>
    /// <param name="markdown">The input string containing Markdown syntax to be converted.</param>
    /// <returns>A string containing the HTML representation of the input Markdown.</returns>
    public string ToHtml(string markdown);
}
