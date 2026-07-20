using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the result of publishing an individual item within a document branch.
/// </summary>
public sealed class DocumentPublishBranchItemResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the document in the publish branch operation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the status of the content publishing operation.
    /// </summary>
    public required ContentPublishingOperationStatus OperationStatus { get; init; }
}
