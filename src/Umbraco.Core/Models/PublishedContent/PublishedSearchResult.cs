using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a search result containing published content and its relevance score.
/// </summary>
[DebuggerDisplay("{Content?.Name} ({Score})")]
public class PublishedSearchResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedSearchResult"/> class.
    /// </summary>
    /// <param name="content">The published content item.</param>
    /// <param name="score">The relevance score of the search result.</param>
    public PublishedSearchResult(IPublishedContent content, float score)
    {
        Content = content;
        Score = score;
    }

    /// <summary>
    ///     Gets the published content item.
    /// </summary>
    public IPublishedContent Content { get; }

    /// <summary>
    ///     Gets the relevance score of the search result.
    /// </summary>
    public float Score { get; }
}
