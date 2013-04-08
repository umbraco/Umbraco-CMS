using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Configuration;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using Umbraco.Core;

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

        /// <summary>
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <returns></returns>
        protected bool ValidateUserApp(string app)
        {
            //ensure we have a valid user first!
            if (!ValidateUser()) return false;
                
            //if it is empty, don't validate
            if (app.IsNullOrWhiteSpace())
            {
                return true;
            }
            return UmbracoUser.Applications.Any(uApp => uApp.alias == app);
        }

        
        private User _user;
        private readonly InnerPage _page = new InnerPage();

        /// <summary>
        /// Returns true if there is a valid logged in user
        /// </summary>
        /// <returns></returns>
        protected bool ValidateUser()
        {
            try
            {
                return UmbracoUser != null;
            }
            catch (ArgumentException)
            {
                //an exception will occur if the user is not valid inside of _page.getUser();
                return false;
            }
            catch (InvalidOperationException)
            {
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
