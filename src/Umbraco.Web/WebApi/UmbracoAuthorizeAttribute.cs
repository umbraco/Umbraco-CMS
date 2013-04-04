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

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
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
    }
}