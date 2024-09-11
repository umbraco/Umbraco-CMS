namespace Umbraco.Cms.Core.Services.OperationStatus;

public sealed class MemberEditingStatus
{
    public ContentEditingOperationStatus ContentEditingOperationStatus { get; set; } = ContentEditingOperationStatus.Unknown;

    public MemberEditingOperationStatus MemberEditingOperationStatus { get; set; } = MemberEditingOperationStatus.Unknown;
}
