using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

[DebuggerDisplay("{Content?.Name} ({Score})")]
public class PublishedSearchResult
{
    public PublishedSearchResult(IPublishedContent content, float score)
    {
        Content = content;
        Score = score;
    }

    public IPublishedContent Content { get; }

    public float Score { get; }
}
