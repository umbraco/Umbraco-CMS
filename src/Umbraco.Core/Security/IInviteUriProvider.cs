using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to create invite URIs for users.
/// </summary>
public interface IInviteUriProvider
{
    /// <summary>
    ///     Creates an invite URI for the specified user.
    /// </summary>
    /// <param name="invitee">The user being invited.</param>
    /// <returns>An attempt containing the generated invite URI or an error status.</returns>
    Task<Attempt<Uri, UserOperationStatus>> CreateInviteUriAsync(IUser invitee);
}
