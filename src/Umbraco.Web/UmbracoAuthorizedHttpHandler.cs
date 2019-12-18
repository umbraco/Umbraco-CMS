using System;
using System.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web.Security;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Web
{
    public abstract class UmbracoAuthorizedHttpHandler : UmbracoHttpHandler
    {
        protected UmbracoAuthorizedHttpHandler()
        {
        }

        protected UmbracoAuthorizedHttpHandler(IUmbracoContextAccessor umbracoContextAccessor, UmbracoHelper umbracoHelper, ServiceContext service, IProfilingLogger plogger) : base(umbracoContextAccessor, umbracoHelper, service, plogger)
        {
        }

        /// <summary>
        /// Checks if the umbraco context id is valid
        /// </summary>
        /// <param name="currentUmbracoUserContextId"></param>
        /// <returns></returns>
        protected bool ValidateUserContextId(string currentUmbracoUserContextId)
        {
            return Security.ValidateCurrentUser();
        }

        /// <summary>
        /// Checks if the username/password credentials are valid
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected bool ValidateCredentials(string username, string password)
        {
            return Security.ValidateBackOfficeCredentials(username, password);
        }

        /// <summary>
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <param name="throwExceptions">true if an exception should be thrown if authorization fails</param>
        /// <returns></returns>
        protected bool AuthorizeRequest(string app, bool throwExceptions = false)
        {
            //ensure we have a valid user first!
            if (!AuthorizeRequest(throwExceptions)) return false;

            //if it is empty, don't validate
            if (app.IsNullOrWhiteSpace())
            {
                return true;
            }
            var hasAccess = UserHasAppAccess(app, Security.CurrentUser);
            if (!hasAccess && throwExceptions)
                throw new SecurityException("The user does not have access to the required application");
            return hasAccess;
        }

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, IUser user)
        {
            return Security.UserHasSectionAccess(app, user);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, string username)
        {
            return Security.UserHasSectionAccess(app, username);
        }

        /// <summary>
        /// Returns true if there is a valid logged in user and that ssl is enabled if required
        /// </summary>
        /// <param name="throwExceptions">true if an exception should be thrown if authorization fails</param>
        /// <returns></returns>
        protected bool AuthorizeRequest(bool throwExceptions = false)
        {
            var result = Security.AuthorizeRequest(throwExceptions);
            return result == ValidateRequestAttempt.Success;
        }


    }
}
