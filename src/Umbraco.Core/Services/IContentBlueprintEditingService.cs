using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides editing services for content blueprints.
/// </summary>
/// <remarks>
///     Content blueprints are templates that can be used to create new content items
///     with pre-populated property values.
/// </remarks>
public interface IContentBlueprintEditingService
{
    /// <summary>
    ///     Gets a content blueprint by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the blueprint.</param>
    /// <returns>The content blueprint, or <c>null</c> if not found.</returns>
    Task<IContent?> GetAsync(Guid key);

    /// <summary>
    ///     Gets a scaffolded content item based on a blueprint.
    /// </summary>
    /// <param name="key">The unique identifier of the blueprint to scaffold from.</param>
    /// <returns>A new content item with values populated from the blueprint, or <c>null</c> if the blueprint is not found.</returns>
    Task<IContent?> GetScaffoldedAsync(Guid key) => Task.FromResult<IContent?>(null);

    /// <summary>
    ///     Gets a paged collection of blueprints for a specific content type.
    /// </summary>
    /// <param name="contentTypeKey">The unique identifier of the content type.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>An attempt containing the paged result of blueprints or an error status.</returns>
    Task<Attempt<PagedModel<IContent>?, ContentEditingOperationStatus>> GetPagedByContentTypeAsync(
        Guid contentTypeKey,
        int skip,
        int take);

    /// <summary>
    ///     Creates a new content blueprint.
    /// </summary>
    /// <param name="createModel">The model containing the blueprint data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateAsync(ContentBlueprintCreateModel createModel, Guid userKey);

    /// <summary>
    ///     Creates a new content blueprint from an existing content item.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content item to create the blueprint from.</param>
    /// <param name="name">The name for the new blueprint.</param>
    /// <param name="key">The optional unique identifier for the new blueprint.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the creation result or an error status.</returns>
    Task<Attempt<ContentCreateResult, ContentEditingOperationStatus>> CreateFromContentAsync(Guid contentKey, string name, Guid? key, Guid userKey);

    /// <summary>
    ///     Updates an existing content blueprint.
    /// </summary>
    /// <param name="key">The unique identifier of the blueprint to update.</param>
    /// <param name="updateModel">The model containing the updated blueprint data.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the update result or an error status.</returns>
    Task<Attempt<ContentUpdateResult, ContentEditingOperationStatus>> UpdateAsync(Guid key, ContentBlueprintUpdateModel updateModel, Guid userKey);

    /// <summary>
    ///     Deletes a content blueprint.
    /// </summary>
    /// <param name="key">The unique identifier of the blueprint to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the deleted blueprint or an error status.</returns>
    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Moves a content blueprint to a different container.
    /// </summary>
    /// <param name="key">The unique identifier of the blueprint to move.</param>
    /// <param name="containerKey">The unique identifier of the target container, or <c>null</c> to move to the root.</param>
    /// <param name="userKey">The unique identifier of the user performing the action.</param>
    /// <returns>An attempt containing the operation status.</returns>
    Task<Attempt<ContentEditingOperationStatus>> MoveAsync(Guid key, Guid? containerKey, Guid userKey);
}
