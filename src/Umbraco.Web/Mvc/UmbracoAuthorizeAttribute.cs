using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Security;
using umbraco.BasePages;

namespace Umbraco.Web.Mvc
{
	/// <summary>	
	/// Ensures authorization is successful for a back office user
	/// </summary>
	public sealed class UmbracoAuthorizeAttribute : AuthorizeAttribute
	{
        private readonly ApplicationContext _applicationContext;
        private readonly UmbracoContext _umbracoContext;

        public UmbracoAuthorizeAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
            _applicationContext = _umbracoContext.Application;
        }

		public UmbracoAuthorizeAttribute()
            : this(UmbracoContext.Current)
		{

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
				//we need to that the app is configured and that a user is logged in
				if (!_applicationContext.IsConfigured)
					return false;
                var isLoggedIn = _umbracoContext.Security.ValidateUserContextId(_umbracoContext.Security.UmbracoUserContextId);
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
            filterContext.Result = (ActionResult)new HttpUnauthorizedResult("You must login to view this resource.");
            
            //DON'T do a FormsAuth redirect... argh!! thankfully we're running .Net 4.5 :)
            filterContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
        }

	}
}