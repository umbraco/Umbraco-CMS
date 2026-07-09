using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to create forgot password URIs for users.
/// </summary>
public interface IForgotPasswordUriProvider
{
    /// <summary>
    ///     Creates a forgot password URI for the specified user.
    /// </summary>
    /// <param name="user">The user to create the forgot password URI for.</param>
    /// <returns>An attempt containing the generated URI or an error status.</returns>
    Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user);
}
