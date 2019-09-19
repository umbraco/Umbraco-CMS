using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Security;
using umbraco.BasePages;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Mvc
{
    /// <summary>	
	/// Ensures authorization is successful for a back office user
	/// </summary>
	public sealed class UmbracoAuthorizeAttribute : AuthorizeAttribute
	{
        private readonly ApplicationContext _applicationContext;
        private readonly UmbracoContext _umbracoContext;
        private readonly string _redirectUrl;

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
        public UmbracoAuthorizeAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _applicationContext = _umbracoContext.Application;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoAuthorizeAttribute()
        {
        }

        /// <summary>
        /// Constructor specifying to redirect to the specified location if not authorized
        /// </summary>
        /// <param name="redirectUrl"></param>
        public UmbracoAuthorizeAttribute(string redirectUrl)
        {
            _redirectUrl = redirectUrl ?? throw new ArgumentNullException(nameof(redirectUrl));
        }

        /// <summary>
        /// Constructor specifying to redirect to the umbraco login page if not authorized
        /// </summary>
        /// <param name="redirectToUmbracoLogin"></param>
        public UmbracoAuthorizeAttribute(bool redirectToUmbracoLogin)
        {
            if (redirectToUmbracoLogin)
            {
                _redirectUrl = GlobalSettings.Path.EnsureStartsWith("~");
            }
        }

        /// <summary>
        /// Ensures that the user must be in the Administrator or the Install role
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
		    if (httpContext == null) throw new ArgumentNullException("httpContext");
            
		    try
			{
                var appContext = GetApplicationContext();
                var umbContext = GetUmbracoContext();

				//we need to that the app is configured and that a user is logged in
                if (!appContext.IsConfigured)
					return false;

                var isLoggedIn = umbContext.Security.ValidateCurrentUser();
				return isLoggedIn;
			}
			catch (Exception)
			{
				return false;
			}
		}

        /// <summary>
        /// Override to to ensure no redirect occurs
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (_redirectUrl.IsNullOrWhiteSpace())
            {
                filterContext.Result = (ActionResult)new HttpUnauthorizedResult("You must login to view this resource.");

                
            }
            else
            {
                filterContext.Result = new RedirectResult(_redirectUrl);
            }

            //DON'T do a FormsAuth redirect... argh!! thankfully we're running .Net 4.5 :)
            filterContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
        }

	}
}
