namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the combined status of a member editing operation, including both content and member-specific statuses.
/// </summary>
public sealed class MemberEditingStatus
{
    /// <summary>
    /// Gets or sets the content editing operation status component.
    /// </summary>
    public ContentEditingOperationStatus ContentEditingOperationStatus { get; set; } = ContentEditingOperationStatus.Unknown;

    /// <summary>
    /// Gets or sets the member editing operation status component.
    /// </summary>
    public MemberEditingOperationStatus MemberEditingOperationStatus { get; set; } = MemberEditingOperationStatus.Unknown;
}
