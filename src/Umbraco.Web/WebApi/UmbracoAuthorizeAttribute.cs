using System;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebApi
{

    // TODO: This has been migrated to netcore and can be removed when ready

    public sealed class UmbracoAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly bool _requireApproval;

        /// <summary>
        /// Can be used by unit tests to enable/disable this filter
        /// </summary>
        internal static bool Enable = true;

        // TODO: inject!
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IRuntimeState _runtimeState;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private IBackofficeSecurity BackofficeSecurity => _backofficeSecurityAccessor.BackofficeSecurity ?? Current.UmbracoContext.Security;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="backofficeSecurityAccessor"></param>
        /// <param name="runtimeState"></param>
        public UmbracoAuthorizeAttribute(IBackofficeSecurityAccessor backofficeSecurityAccessor, IRuntimeState runtimeState)
        {
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
        }

        public UmbracoAuthorizeAttribute() : this(true)
        { }

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
                // if not configured (install or upgrade) then we can continue
                // otherwise we need to ensure that a user is logged in
                return RuntimeState.Level == RuntimeLevel.Install
                    || RuntimeState.Level == RuntimeLevel.Upgrade
                    || BackofficeSecurity.ValidateCurrentUser(false, _requireApproval) == ValidateRequestAttempt.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
