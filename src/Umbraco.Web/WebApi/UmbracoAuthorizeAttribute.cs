﻿using System;
using System.Web.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Web.Composing;

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
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IRuntimeState _runtimeState;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private IBackOfficeSecurity BackOfficeSecurity => _backOfficeSecurityAccessor.BackOfficeSecurity ?? Current.BackOfficeSecurityAccessor.BackOfficeSecurity;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="backofficeSecurityAccessor"></param>
        /// <param name="runtimeState"></param>
        public UmbracoAuthorizeAttribute(IBackOfficeSecurityAccessor backofficeSecurityAccessor, IRuntimeState runtimeState)
        {
            _backOfficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
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

            // if not configured (install or upgrade) then we can continue
            // otherwise we need to ensure that a user is logged in

            switch (_runtimeState.Level)
            {
                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:
                    return true;
                default:
                    var userApprovalSucceeded = !_requireApproval || (BackOfficeSecurity.CurrentUser?.IsApproved ?? false);
                    return userApprovalSucceeded;
            }
        }
    }
}
