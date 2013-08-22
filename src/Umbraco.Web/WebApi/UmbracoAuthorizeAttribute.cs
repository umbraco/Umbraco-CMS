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

        public UmbracoAuthorizeAttribute()
        {
        }

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            try
            {
                //we need to that the app is configured and that a user is logged in
                if (!GetApplicationContext().IsConfigured)
                    return false;
                var umbCtx = GetUmbracoContext();
                var isLoggedIn = umbCtx.Security.ValidateUserContextId(umbCtx.Security.UmbracoUserContextId);
                return isLoggedIn;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}