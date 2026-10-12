namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the combined status of an operation that both saves and publishes content.
/// </summary>
public sealed class ContentEditingAndPublishingStatus
{
    /// <summary>
    /// Gets the status of the save part of the operation.
    /// </summary>
    public ContentEditingOperationStatus ContentEditingOperationStatus { get; init; } = ContentEditingOperationStatus.Unknown;

    /// <summary>
    /// Gets the status of the publish part of the operation, or <c>null</c> when publishing was not attempted
    /// because the save did not complete.
    /// </summary>
    public ContentPublishingOperationStatus? ContentPublishingOperationStatus { get; init; }
}
