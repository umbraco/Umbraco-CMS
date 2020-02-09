using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Security
{
    public interface IWebSecurity
    {
        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>The current user.</value>
        IUser CurrentUser { get; }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <returns>returns the number of seconds until their session times out</returns>
        double PerformLogin(int userId);

        /// <summary>
        /// Clears the current login for the currently logged in user
        /// </summary>
        void ClearCurrentLogin();

        /// <summary>
        /// Validates credentials for a back office user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <remarks>
        /// This uses ASP.NET Identity to perform the validation
        /// </remarks>
        bool ValidateBackOfficeCredentials(string username, string password);

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns></returns>
        Attempt<int> GetUserId();

        /// <summary>
        /// Returns the current user's unique session id - used to mitigate csrf attacks or any other reason to validate a request
        /// </summary>
        /// <returns></returns>
        string GetSessionId();

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
        /// Authorizes the full request, checks for SSL and validates the current user
        /// </summary>
        /// <param name="throwExceptions">set to true if you want exceptions to be thrown if failed</param>
        /// <returns></returns>
        ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false);

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UserHasSectionAccess(string section, IUser user);

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="section"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        bool UserHasSectionAccess(string section, string username);

        /// <summary>
        /// Ensures that a back office user is logged in
        /// </summary>
        /// <returns></returns>
        bool IsAuthenticated();
    }
}
