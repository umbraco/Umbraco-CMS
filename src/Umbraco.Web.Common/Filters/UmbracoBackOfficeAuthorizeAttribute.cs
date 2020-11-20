using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Ensures authorization is successful for a back office user.
    /// </summary>
    public class UmbracoBackOfficeAuthorizeAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoBackOfficeAuthorizeAttribute() : this(false, false)
        {
        }

        /// <summary>
        /// Constructor with redirect umbraco login behavior
        /// </summary>
        /// <param name="redirectToUmbracoLogin"></param>
        /// <param name="requireApproval"></param>

        public UmbracoBackOfficeAuthorizeAttribute(bool redirectToUmbracoLogin, bool requireApproval) : base(typeof(UmbracoBackOfficeAuthorizeFilter))
        {
            Arguments = new object[] { redirectToUmbracoLogin, requireApproval };
        }

        /// <summary>
        /// Constructor with redirect url behavior
        /// </summary>
        /// <param name="redirectUrl"></param>
        public UmbracoBackOfficeAuthorizeAttribute(string redirectUrl) : base(typeof(UmbracoBackOfficeAuthorizeFilter))
        {
            Arguments = new object[] { redirectUrl };
        }

        /// <summary>
        /// Ensures authorization is successful for a back office user.
        /// </summary>
        private class UmbracoBackOfficeAuthorizeFilter : IAuthorizationFilter
        {
            private readonly bool _requireApproval;

            /// <summary>
            /// Can be used by unit tests to enable/disable this filter
            /// </summary>
            internal static bool Enable = true;
            private readonly IHostingEnvironment _hostingEnvironment;
            private readonly IUmbracoContextAccessor _umbracoContext;
            private readonly IRuntimeState _runtimeState;
            private readonly LinkGenerator _linkGenerator;
            // TODO: This should really be refactored. Redirection/Challenge is part of Authentication, not part of authorization directly
            // We only use this redirectToUmbracoLogin flag in this one instance. I think this
            // should be handled as part of the preview authentication process instead. 
            private readonly bool _redirectToUmbracoLogin;
            private string _redirectUrl;

            private UmbracoBackOfficeAuthorizeFilter(
                IHostingEnvironment hostingEnvironment,
                IUmbracoContextAccessor umbracoContext,
                IRuntimeState runtimeState, LinkGenerator linkGenerator,
                bool redirectToUmbracoLogin, bool requireApproval, string redirectUrl)
            {
                _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
                _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
                _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
                _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
                _redirectToUmbracoLogin = redirectToUmbracoLogin;
                _redirectUrl = redirectUrl;
                _requireApproval = requireApproval;
            }

            /// <summary>
            /// Default constructor
            /// </summary>
            /// <param name="hostingEnvironment"></param>
            /// <param name="umbracoContext"></param>
            /// <param name="runtimeState"></param>
            /// <param name="linkGenerator"></param>
            /// <param name="redirectUrl"></param>
            public UmbracoBackOfficeAuthorizeFilter(
                IHostingEnvironment hostingEnvironment,
                IUmbracoContextAccessor umbracoContext,
                IRuntimeState runtimeState, LinkGenerator linkGenerator,
                string redirectUrl) : this(hostingEnvironment, umbracoContext, runtimeState, linkGenerator, false, false, redirectUrl)
            {
            }

            public UmbracoBackOfficeAuthorizeFilter(
                IHostingEnvironment hostingEnvironment,
                IUmbracoContextAccessor umbracoContext,
                IRuntimeState runtimeState, LinkGenerator linkGenerator,
                bool redirectToUmbracoLogin, bool requireApproval) : this(hostingEnvironment, umbracoContext, runtimeState, linkGenerator, redirectToUmbracoLogin, requireApproval, null)
            {
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!IsAuthorized())
                {
                    if (_redirectToUmbracoLogin)
                    {
                        _redirectUrl = _linkGenerator.GetBackOfficeUrl(_hostingEnvironment);
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
}
