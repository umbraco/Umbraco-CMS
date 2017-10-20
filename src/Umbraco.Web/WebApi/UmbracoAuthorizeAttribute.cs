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
        private readonly bool _requireApproval;

        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

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

        public UmbracoAuthorizeAttribute() : this(true)
        {
            
        }

        public UmbracoAuthorizeAttribute(bool requireApproval)
        {
            _requireApproval = requireApproval;
        }

        protected override bool IsAuthorized(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (Enable == false)
            {
                return true;
            }

            try
            {
                var appContext = GetApplicationContext();
                var umbContext = GetUmbracoContext();

                //we need to that the app is configured and that a user is logged in
                if (appContext.IsConfigured == false)
                    return false;

                var isLoggedIn = umbContext.Security.ValidateCurrentUser(false, _requireApproval) == ValidateRequestAttempt.Success;

                return isLoggedIn;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}