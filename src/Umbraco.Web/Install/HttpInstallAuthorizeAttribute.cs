using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Web.Security;
using umbraco.BasePages;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Install
{
	/// <summary>
	/// Ensures authorization occurs for the installer if it has already completed. If install has not yet occured
	/// then the authorization is successful
	/// </summary>
	internal class HttpInstallAuthorizeAttribute : AuthorizeAttribute
	{
        private readonly ApplicationContext _applicationContext;
        private readonly UmbracoContext _umbracoContext;

        private ApplicationContext GetApplicationContext()
        {
            return _applicationContext ?? ApplicationContext.Current;
        }

        private UmbracoContext GetUmbracoContext()
        {
            return _umbracoContext ?? UmbracoContext.Current;
        }

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        public HttpInstallAuthorizeAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _applicationContext = _umbracoContext.Application;
        }

        public HttpInstallAuthorizeAttribute()
		{			
        }

	    protected override bool IsAuthorized(HttpActionContext actionContext)
	    {
            try
            {
                //if its not configured then we can continue
                if (GetApplicationContext().IsConfigured == false)
                {
                    return true;
                }
                var umbCtx = GetUmbracoContext();

                //otherwise we need to ensure that a user is logged in
                var isLoggedIn = GetUmbracoContext().Security.ValidateCurrentUser();
                if (isLoggedIn)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Error<HttpInstallAuthorizeAttribute>("An error occurred determining authorization", ex);
                return false;
            }
	    }
       
	}
}