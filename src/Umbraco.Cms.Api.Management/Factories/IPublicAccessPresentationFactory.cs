using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory for creating public access presentation models.
/// </summary>
public interface IPublicAccessPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="PublicAccessResponseModel"/> from a <see cref="PublicAccessEntry"/>.
    /// </summary>
    /// <param name="entry">The public access entry.</param>
    /// <param name="contentKey">The key of the content item being queried, used to determine if protection is inherited from an ancestor.</param>
    /// <returns>An <see cref="Attempt{TResult, TStatus}"/> containing the response model or an error status.</returns>
    /// <remarks>
    /// Determines whether the entry is inherited from an ancestor by comparing the entry's protected node key against
    /// <paramref name="contentKey"/>.
    /// </remarks>
    // TODO (V18): Remove the default implementation.
    Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry, Guid contentKey)
#pragma warning disable CS0618 // Type or member is obsolete
        => CreatePublicAccessResponseModel(entry);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Creates a <see cref="PublicAccessResponseModel"/> from a <see cref="PublicAccessEntry"/>.
    /// </summary>
    /// <param name="entry">The public access entry.</param>
    /// <returns>An <see cref="Attempt{TResult, TStatus}"/> containing the response model or an error status.</returns>
    [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 19.")]
    Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> CreatePublicAccessResponseModel(PublicAccessEntry entry);

    /// <summary>
    /// Creates a <see cref="PublicAccessEntrySlim"/> from a <see cref="PublicAccessRequestModel"/>.
    /// </summary>
    /// <param name="requestModel">The public access request model.</param>
    /// <param name="contentKey">The key of the content item to protect.</param>
    /// <returns>A <see cref="PublicAccessEntrySlim"/> representing the public access entry.</returns>
    PublicAccessEntrySlim CreatePublicAccessEntrySlim(PublicAccessRequestModel requestModel, Guid contentKey);
}
