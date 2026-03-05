namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents the result of a content branch publishing operation.
/// </summary>
public sealed class ContentPublishingBranchResult
{
    /// <summary>
    ///     Gets or initializes the root content item of the branch that was processed.
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
