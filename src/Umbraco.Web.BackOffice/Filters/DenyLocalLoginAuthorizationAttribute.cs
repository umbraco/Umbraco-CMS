using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.Editors.Filters
{
    [UmbracoVolatile]
    public sealed class DenyLocalLoginAuthorizationAttribute : TypeFilterAttribute
    {
        public DenyLocalLoginAuthorizationAttribute() : base(typeof(DenyLocalLoginFilter))
        {
        }

        private class DenyLocalLoginFilter : IAuthorizationFilter
        {
            private readonly IBackOfficeExternalLoginProviders _externalLogins;

            public DenyLocalLoginFilter(IBackOfficeExternalLoginProviders externalLogins)
            {
                _externalLogins = externalLogins;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (_externalLogins.HasDenyLocalLogin())
                {
                    // if there is a deny local login provider then we cannot authorize
                    context.Result = new ForbidResult();
                }
            }
        }

    }
}
