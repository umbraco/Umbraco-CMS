namespace Umbraco.Cms.Core.Models.ContentPublishing;

public sealed class ContentPublishingBranchResult
{
    public IContent? Content { get; init; }

    public IEnumerable<ContentPublishingBranchItemResult> SucceededItems { get; set; } = [];

    public IEnumerable<ContentPublishingBranchItemResult> FailedItems { get; set; } = [];
}
