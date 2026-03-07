using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating presentation models related to public access in the management API.
/// </summary>
public interface IPublicAccessPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="PublicAccessResponseModel"/> from the specified <see cref="PublicAccessEntry"/>.
    /// </summary>
    /// <param name="entry">The public access entry to create the response model from.</param>
    /// <returns>An <see cref="Attempt{PublicAccessResponseModel?, PublicAccessOperationStatus}"/> representing the result of the creation operation.</returns>
    Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry);

    PublicAccessEntrySlim CreatePublicAccessEntrySlim(PublicAccessRequestModel requestModel, Guid contentKey);
}
