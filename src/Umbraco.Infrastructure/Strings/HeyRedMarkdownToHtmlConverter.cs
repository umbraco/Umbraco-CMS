using HeyRed.MarkdownSharp;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.Strings;

// TODO (V19): Remove this class along with the HeyRed.MarkdownSharp library entirely (remove reference from Directory.props and .csproj, and remove from NOTICES.txt).

/// <summary>
/// Implements a service that converts Markdown-formatted text to HTML using the HeyRed.MarkdownSharp library.
/// </summary>
[Obsolete("Uses the deprecated HeyRed.MarkdownSharp library which will continue to be provided for the lifetime of Umbraco 17 as the default implementation of IMarkdownToHtmlConverter. The default will be changed to MarkdigMarkdownToHtmlConverter for Umbraco 18. Scheduled for removal along with the HeyRed.MarkdownSharp library in Umbraco 19.")]
public class HeyRedMarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    private static readonly Markdown _markdownConverter = new();

    /// <inheritdoc/>
    public string ToHtml(string markdown) => _markdownConverter.Transform(markdown);
}
