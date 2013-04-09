using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using Umbraco.Core;
using umbraco.businesslogic.Exceptions;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// An abstract web service class that has the methods and properties to correct validate an Umbraco user
    /// </summary>
    public abstract class UmbracoAuthorizedWebService : UmbracoWebService
    {
        protected UmbracoAuthorizedWebService()
            : base()
        {
        }

        protected UmbracoAuthorizedWebService(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }
        
        //IMPORTANT NOTE: !! All of these security bits and pieces have been moved in to one centralized class
        // in 6.1 called WebSecurity. All this logic is all here temporarily!

        private User _user;
        private readonly InnerPage _page = new InnerPage();

        /// <summary>
        /// Checks if the umbraco context id is valid
        /// </summary>
        /// <param name="currentUmbracoUserContextId"></param>
        /// <returns></returns>
        protected bool ValidateUserContextId(string currentUmbracoUserContextId)
        {
            return BasePage.ValidateUserContextID(currentUmbracoUserContextId);
        }

        /// <summary>
        /// Checks if the username/password credentials are valid
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected bool ValidateCredentials(string username, string password)
        {
            return Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].ValidateUser(username, password);
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
            var hasAccess = UserHasAppAccess(app, UmbracoUser);
            if (!hasAccess && throwExceptions)
                throw new UserAuthorizationException("The user does not have access to the required application");
            return hasAccess;
        }

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, User user)
        {
            return user.Applications.Any(uApp => uApp.alias == app);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, string username)
        {
            var uid = global::umbraco.BusinessLogic.User.getUserId(username);
            if (uid < 0) return false;
            var usr = global::umbraco.BusinessLogic.User.GetUser(uid);
            if (usr == null) return false;
            return UserHasAppAccess(app, usr);
        }

        /// <summary>
        /// Returns true if there is a valid logged in user and that ssl is enabled if required
        /// </summary>
        /// <param name="throwExceptions">true if an exception should be thrown if authorization fails</param>
        /// <returns></returns>
        protected bool AuthorizeRequest(bool throwExceptions = false)
        {
            // check for secure connection
            if (GlobalSettings.UseSSL && !HttpContext.Current.Request.IsSecureConnection)
            {
                if (throwExceptions)
                    throw new UserAuthorizationException("This installation requires a secure connection (via SSL). Please update the URL to include https://");
                return false;
            }                

            try
            {
                return UmbracoUser != null;
            }
            catch (ArgumentException)
            {
                if (throwExceptions) throw;
                //an exception will occur if the user is not valid inside of _page.getUser();
                return false;
            }
            catch (InvalidOperationException)
            {
                if (throwExceptions) throw;
                //an exception will occur if the user is not valid inside of _page.getUser();
                return false;
            }
        }

        /// <summary>
        /// Returns the current user
        /// </summary>
        protected User UmbracoUser
        {
            get
            {
                return _user ?? (_user = _page.getUser());
            }
        }

        /// <summary>
        /// Used to validate, thie is temporary, in 6.1 we have the WebSecurity class which does all 
        /// authorization stuff for us.
        /// </summary>
        private class InnerPage : BasePage
        {
            
        }

    }
}
