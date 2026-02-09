using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the member editing service, which provides operations for creating, updating, and deleting members.
/// </summary>
public interface IMemberEditingService
{
    /// <summary>
    ///     Gets a member by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the member.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IMember" /> if found; otherwise, null.</returns>
    Task<IMember?> GetAsync(Guid key);

    /// <summary>
    ///     Validates a member create model before creation.
    /// </summary>
    /// <param name="createModel">The <see cref="MemberCreateModel" /> to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with validation results and operation status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateCreateAsync(MemberCreateModel createModel);

    /// <summary>
    ///     Validates a member update model before updating.
    /// </summary>
    /// <param name="key">The unique key of the member to update.</param>
    /// <param name="updateModel">The <see cref="MemberUpdateModel" /> to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with validation results and operation status.</returns>
    Task<Attempt<ContentValidationResult, ContentEditingOperationStatus>> ValidateUpdateAsync(Guid key, MemberUpdateModel updateModel);

    /// <summary>
    ///     Creates a new member.
    /// </summary>
    /// <param name="createModel">The <see cref="MemberCreateModel" /> containing the member data.</param>
    /// <param name="user">The user performing the create operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the creation result and status.</returns>
    Task<Attempt<MemberCreateResult, MemberEditingStatus>> CreateAsync(MemberCreateModel createModel, IUser user);

    /// <summary>
    ///     Updates an existing member.
    /// </summary>
    /// <param name="key">The unique key of the member to update.</param>
    /// <param name="updateModel">The <see cref="MemberUpdateModel" /> containing the updated member data.</param>
    /// <param name="user">The user performing the update operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the update result and status.</returns>
    Task<Attempt<MemberUpdateResult, MemberEditingStatus>> UpdateAsync(Guid key, MemberUpdateModel updateModel, IUser user);

    /// <summary>
    ///     Deletes a member.
    /// </summary>
    /// <param name="key">The unique key of the member to delete.</param>
    /// <param name="userKey">The unique key of the user performing the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{TResult,TStatus}" /> with the deleted member and status.</returns>
    Task<Attempt<IMember?, MemberEditingStatus>> DeleteAsync(Guid key, Guid userKey);
}
