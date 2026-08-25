using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Extensions;

/// <summary>
/// Extension methods for <see cref="MemberResponseModel"/>.
/// </summary>
internal static class MemberResponseModelExtensions
{
    /// <summary>
    /// Clears the member account state that is subject to "sensitive data" rules, unless the user is
    /// permitted to see it.
    /// </summary>
    /// <param name="responseModel">The response model to clear the values on.</param>
    /// <param name="currentUser">The user the response model is destined for.</param>
    /// <remarks>
    /// Every response model carrying member account state must be passed through this before it reaches the
    /// user, whichever endpoint produced it. Some of the fields are not nullable, so for those we can't do
    /// much more than force revert them to their default values - which means a default value in a response
    /// is not evidence of the member's actual state.
    /// </remarks>
    public static void ClearSensitiveValuesFor(this MemberResponseModel responseModel, IUser currentUser)
    {
        if (currentUser.HasAccessToSensitiveData())
        {
            return;
        }

        responseModel.IsApproved = false;
        responseModel.IsLockedOut = false;
        responseModel.IsTwoFactorEnabled = false;
        responseModel.FailedPasswordAttempts = 0;
        responseModel.LastLoginDate = null;
        responseModel.LastLockoutDate = null;
        responseModel.LastPasswordChangeDate = null;
    }
}
