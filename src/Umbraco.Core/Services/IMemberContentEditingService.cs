using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides content editing operations for members.
/// </summary>
public interface IMemberContentEditingService
{
    /// <summary>
    ///     Validates a member editing model against its member type.
    /// </summary>
    /// <param name="editingModel">The member editing model to validate.</param>
    /// <param name="memberTypeKey">The unique identifier of the member type.</param>
    /// <returns>
    ///     An attempt containing the <see cref="ContentValidationResult"/> if successful,
    ///     or a <see cref="ContentEditingOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateAsync(MemberEditingModelBase editingModel, Guid memberTypeKey);

    /// <summary>
    ///     Updates an existing member with the provided editing model.
    /// </summary>
    /// <param name="member">The member to update.</param>
    /// <param name="updateModel">The model containing the updated member data.</param>
    /// <param name="userKey">The unique identifier of the user performing the update.</param>
    /// <returns>
    ///     An attempt containing the <see cref="MemberUpdateResult"/> if successful,
    ///     or a <see cref="ContentEditingOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<MemberUpdateResult, ContentEditingOperationStatus>> UpdateAsync(IMember member, MemberEditingModelBase updateModel, Guid userKey);

    /// <summary>
    ///     Deletes a member by its unique identifier.
    /// </summary>
    /// <param name="key">The unique identifier of the member to delete.</param>
    /// <param name="userKey">The unique identifier of the user performing the deletion.</param>
    /// <returns>
    ///     An attempt containing the deleted <see cref="IMember"/> if successful,
    ///     or a <see cref="ContentEditingOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<IMember?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);
}
