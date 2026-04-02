using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Service interface for creating and managing member types in the Umbraco backoffice.
/// </summary>
/// <remarks>
///     This service provides operations for creating, updating, and querying member types,
///     including support for compositions and member-specific property configurations
///     such as sensitivity and visibility settings.
/// </remarks>
public interface IMemberTypeEditingService
{
    /// <summary>
    ///     Creates a new member type based on the provided model.
    /// </summary>
    /// <param name="model">The model containing the member type definition including properties and compositions.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the created <see cref="IMemberType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IMemberType?, ContentTypeOperationStatus>> CreateAsync(MemberTypeCreateModel model, Guid userKey);

    /// <summary>
    ///     Updates an existing member type with the provided model data.
    /// </summary>
    /// <param name="memberType">The existing member type to update.</param>
    /// <param name="model">The model containing the updated member type definition.</param>
    /// <param name="userKey">The unique identifier of the user performing the operation.</param>
    /// <returns>
    ///     An <see cref="Attempt{TResult,TStatus}"/> containing the updated <see cref="IMemberType"/> on success,
    ///     or a <see cref="ContentTypeOperationStatus"/> indicating the reason for failure.
    /// </returns>
    Task<Attempt<IMemberType?, ContentTypeOperationStatus>> UpdateAsync(IMemberType memberType, MemberTypeUpdateModel model, Guid userKey);

    /// <summary>
    ///     Gets the available compositions for a member type.
    /// </summary>
    /// <param name="key">The unique identifier of the member type, or <c>null</c> for a new member type.</param>
    /// <param name="currentCompositeKeys">The keys of currently selected compositions.</param>
    /// <param name="currentPropertyAliases">The aliases of properties currently defined on the member type.</param>
    /// <returns>
    ///     A collection of <see cref="ContentTypeAvailableCompositionsResult"/> indicating which compositions
    ///     are available and which are not allowed due to conflicts.
    /// </returns>
    Task<IEnumerable<ContentTypeAvailableCompositionsResult>> GetAvailableCompositionsAsync(
        Guid? key,
        IEnumerable<Guid> currentCompositeKeys,
        IEnumerable<string> currentPropertyAliases);
}
