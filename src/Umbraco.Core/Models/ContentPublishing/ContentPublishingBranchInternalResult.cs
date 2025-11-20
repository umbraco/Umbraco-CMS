namespace Umbraco.Cms.Core.Models.ContentPublishing;

internal sealed class ContentPublishingBranchInternalResult
{
    public Guid? ContentKey { get; init; }

    public IContent? Content { get; init; }

    public IEnumerable<ContentPublishingBranchItemResult> SucceededItems { get; set; } = [];

    public IEnumerable<ContentPublishingBranchItemResult> FailedItems { get; set; } = [];

    public Guid? AcceptedTaskId { get; init; }
}
