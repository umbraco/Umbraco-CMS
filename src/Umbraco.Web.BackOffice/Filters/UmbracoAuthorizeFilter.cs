using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{

    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoAuthorizeFilter : IAuthorizationFilter
    {
        private readonly bool _requireApproval;

        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;
        
        private readonly IUmbracoContextAccessor _umbracoContext;
        private readonly IRuntimeState _runtimeState;
        private readonly LinkGenerator _linkGenerator;
        private readonly bool _redirectToUmbracoLogin;
        private string _redirectUrl;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        /// <param name="linkGenerator"></param>
        public UmbracoAuthorizeFilter(
            IUmbracoContextAccessor umbracoContext, IRuntimeState runtimeState, LinkGenerator linkGenerator)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        }

        /// <summary>
        /// Constructor with redirect umbraco login behavior
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        /// <param name="linkGenerator"></param>
        /// <param name="redirectToUmbracoLogin">If true will redirect to the umbraco login page if not authorized</param>
        public UmbracoAuthorizeFilter(
            IUmbracoContextAccessor umbracoContext, IRuntimeState runtimeState, LinkGenerator linkGenerator,
            bool redirectToUmbracoLogin)
            : this(umbracoContext, runtimeState, linkGenerator)
        {            
            _redirectToUmbracoLogin = redirectToUmbracoLogin;
        }

        /// <summary>
        /// Constructor with redirect url behavior
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        /// /// <param name="linkGenerator"></param>
        /// <param name="redirectUrl">If specified will redirect to this URL if not authorized</param>
        public UmbracoAuthorizeFilter(
            IUmbracoContextAccessor umbracoContext, IRuntimeState runtimeState, LinkGenerator linkGenerator,
            string redirectUrl)
            : this(umbracoContext, runtimeState, linkGenerator)
        {
            _redirectUrl = redirectUrl;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IsAuthorized())
            {
                if (_redirectToUmbracoLogin)
                {
                    _redirectUrl = _linkGenerator.GetBackOfficeUrl();
                }

                if (!_redirectUrl.IsNullOrWhiteSpace())
                {
                    context.Result = new RedirectResult(_redirectUrl);
                }
                else
                {
                    context.Result = new ForbidResult();
                }
            }
        }

        private bool IsAuthorized()
        {
            if (Enable == false)
                return true;

            try
            {
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return _runtimeState.Level == RuntimeLevel.Install
                    || _runtimeState.Level == RuntimeLevel.Upgrade
                    || _umbracoContext.UmbracoContext?.Security.ValidateCurrentUser(false, _requireApproval) == ValidateRequestAttempt.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
