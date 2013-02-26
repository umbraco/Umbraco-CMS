using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Ensures authorization is successful for a back office user
    /// </summary>
    public sealed class UmbracoAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly ApplicationContext _applicationContext;

        public UmbracoAuthorizeAttribute(ApplicationContext appContext)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            _applicationContext = appContext;
        }

        public UmbracoAuthorizeAttribute()
            : this(ApplicationContext.Current)
        {

        }

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            try
            {
                //we need to that the app is configured and that a user is logged in
                if (!_applicationContext.IsConfigured)
                    return false;
                var isLoggedIn = WebSecurity.ValidateUserContextId(WebSecurity.UmbracoUserContextId);
                return isLoggedIn;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}