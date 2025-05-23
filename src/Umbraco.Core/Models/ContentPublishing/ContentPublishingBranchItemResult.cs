using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Models.ContentPublishing;

public sealed class ContentPublishingBranchItemResult
{
    public required Guid Key { get; init; }

    public required ContentPublishingOperationStatus OperationStatus { get; init; }
}
