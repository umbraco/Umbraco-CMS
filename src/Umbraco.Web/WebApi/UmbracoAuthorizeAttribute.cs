using System;
using System.Web.Http;
using Umbraco.Core;
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
        private readonly IUmbracoContext _umbracoContext;
        private readonly IRuntimeState _runtimeState;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private IUmbracoContext UmbracoContext => _umbracoContext ?? Current.UmbracoContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="runtimeState"></param>
        public UmbracoAuthorizeAttribute(IUmbracoContext umbracoContext, IRuntimeState runtimeState)
        {
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            _umbracoContext = umbracoContext;
            _runtimeState = runtimeState;
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
                    || UmbracoContext.Security.ValidateCurrentUser(false, _requireApproval) == ValidateRequestAttempt.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
