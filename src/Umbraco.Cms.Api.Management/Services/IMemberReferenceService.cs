using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Service for retrieving paged references to a member, handling the external member fallback
/// when the entity-based lookup fails (external members have no <c>umbracoNode</c> entry).
/// </summary>
public interface IMemberReferenceService
{
    /// <summary>
    /// Gets a paged list of items that reference the specified member.
    /// </summary>
    /// <param name="id">The unique identifier of the member.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <returns>An <see cref="Attempt{TResult,TStatus}"/> containing the paged relation items or an operation status on failure.</returns>
    Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedReferencesAsync(Guid id, int skip, int take);
}
