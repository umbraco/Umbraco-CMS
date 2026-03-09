using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents the result of publishing an individual item within a content branch operation.
/// </summary>
public sealed class ContentPublishingBranchItemResult
{
    /// <summary>
    ///     Gets the unique key of the content item.
    /// </summary>
    public required Guid Key { get; init; }

    /// <summary>
    ///     Gets the operation status indicating the result of the publishing operation for this item.
    /// </summary>
    public required ContentPublishingOperationStatus OperationStatus { get; init; }
}
