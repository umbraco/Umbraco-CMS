using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services.References;

/// <summary>
/// Looks up the elements directly referenced (via an Element Picker property, or as embedded reusable block
/// content) by a document or element, that are not fully published.
/// </summary>
public interface IReferencedElementsService
{
    /// <summary>
    /// Gets a paged list of the elements directly referenced by the entity identified by <paramref name="parentKey"/>
    /// that are in a draft, pending-changes, or scheduled state.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the referencing document or element.</param>
    /// <param name="parentObjectType">The object type of the referencing entity (<see cref="UmbracoObjectTypes.Document"/> or <see cref="UmbracoObjectTypes.Element"/>).</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The maximum number of items to return for paging. <c>0</c> returns no items but a correct total.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paged list of the referenced elements that are not fully published.</returns>
    Task<Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus>> GetPagedReferencedElementsWithPendingChangesAsync(
        Guid parentKey,
        UmbracoObjectTypes parentObjectType,
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
