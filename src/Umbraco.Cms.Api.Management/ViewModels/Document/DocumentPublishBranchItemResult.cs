using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public sealed class DocumentPublishBranchItemResult
{
    public required Guid Id { get; init; }

    public required ContentPublishingOperationStatus OperationStatus { get; init; }
}
