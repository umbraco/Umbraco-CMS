namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents the internal result of a content branch publishing operation.
/// </summary>
internal sealed class ContentPublishingBranchInternalResult
{
    /// <summary>
    ///     Gets or initializes the unique key of the content item.
    /// </summary>
    public Guid? ContentKey { get; init; }

    /// <summary>
    ///     Gets or initializes the content item that was processed.
    /// </summary>
    public IContent? Content { get; init; }

    /// <summary>
    ///     Gets or sets the collection of successfully published items in the branch.
    /// </summary>
    public IEnumerable<ContentPublishingBranchItemResult> SucceededItems { get; set; } = [];

    /// <summary>
    ///     Gets or sets the collection of items that failed to publish in the branch.
    /// </summary>
    public IEnumerable<ContentPublishingBranchItemResult> FailedItems { get; set; } = [];

    /// <summary>
    ///     Gets or initializes the unique identifier of the accepted background task, if any.
    /// </summary>
    public Guid? AcceptedTaskId { get; init; }
}
