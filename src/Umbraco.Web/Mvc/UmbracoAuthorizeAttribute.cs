using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public sealed class UmbracoAuthorizeAttribute : AuthorizeAttribute
    {
        // see note in HttpInstallAuthorizeAttribute
        private readonly UmbracoContext _umbracoContext;
        private readonly IRuntimeState _runtimeState;
        private readonly string _redirectUrl;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private UmbracoContext UmbracoContext => _umbracoContext ?? Current.UmbracoContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        public UmbracoAuthorizeAttribute(UmbracoContext umbracoContext, IRuntimeState runtimeState)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            _umbracoContext = umbracoContext;
            _runtimeState = runtimeState;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoAuthorizeAttribute()
        { }

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
                _redirectUrl = Current.Configs.Global().Path.EnsureStartsWith("~");
            }
        }

        /// <summary>
        /// Ensures that the user must be in the Administrator or the Install role
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return RuntimeState.Level == RuntimeLevel.Install
                    || RuntimeState.Level == RuntimeLevel.Upgrade
                    || UmbracoContext.Security.ValidateCurrentUser();
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

            // DON'T do a FormsAuth redirect... argh!! thankfully we're running .Net 4.5 :)
            filterContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
        }

    }
}
