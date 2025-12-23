using Markdig;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.Strings;

/// <summary>
/// Implements a service that converts Markdown-formatted text to HTML using the Markdig library.
/// </summary>
public class MarkdigMarkdownToHtmlConverter : IMarkdownToHtmlConverter
{
    private static readonly MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().Build();

    /// <inheritdoc/>
    public string ToHtml(string markdown) => Markdown.ToHtml(markdown, _markdownPipeline);
}
