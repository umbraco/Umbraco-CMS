using System;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Security
{
    public interface IBackOfficeSecurity
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        IUser CurrentUser { get; }

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns></returns>
        Attempt<int> GetUserId();

        /// <summary>
        /// Validates the currently logged in user and ensures they are not timed out
        /// </summary>
        /// <returns></returns>
        bool ValidateCurrentUser();

        /// <summary>
        /// Validates the current user assigned to the request and ensures the stored user data is valid
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <param name="requiresApproval">If true requires that the user is approved to be validated</param>
        /// <returns></returns>
        ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true);

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UserHasSectionAccess(string section, IUser user);

        /// <summary>
        /// Ensures that a back office user is logged in
        /// </summary>
        /// <returns></returns>
        bool IsAuthenticated();
    }
}
