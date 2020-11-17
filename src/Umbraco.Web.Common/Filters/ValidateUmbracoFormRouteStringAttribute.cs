﻿using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Web.Common.Constants;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.Security;

namespace Umbraco.Web.Common.Filters
{

    /// <summary>
    /// Attribute used to check that the request contains a valid Umbraco form request string.
    /// </summary>
    /// <remarks>
    /// Applying this attribute/filter to a <see cref="SurfaceController"/> or SurfaceController Action will ensure that the Action can only be executed
    /// when it is routed to from within Umbraco, typically when rendering a form with BeginUmbracoForm. It will mean that the natural MVC route for this Action
    /// will fail with a <see cref="HttpUmbracoFormRouteStringException"/>.
    /// </remarks>
    public class ValidateUmbracoFormRouteStringAttribute : TypeFilterAttribute
    {

        public ValidateUmbracoFormRouteStringAttribute() : base(typeof(ValidateUmbracoFormRouteStringFilter))
        {
            Arguments = new object[] { };
        }

        internal class ValidateUmbracoFormRouteStringFilter: IAuthorizationFilter
        {
            private readonly IDataProtectionProvider _dataProtectionProvider;

            public ValidateUmbracoFormRouteStringFilter(IDataProtectionProvider dataProtectionProvider)
            {
                _dataProtectionProvider = dataProtectionProvider;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));

                var ufprt = context.HttpContext.Request.Form["ufprt"];

                if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    ValidateRouteString(ufprt, controllerActionDescriptor.ControllerName, controllerActionDescriptor.ActionName, context.RouteData?.DataTokens["area"]?.ToString());
                }

            }

            public void ValidateRouteString(string ufprt, string currentController, string currentAction, string currentArea)
            {
                if (ufprt.IsNullOrWhiteSpace())
                {
                    throw new HttpUmbracoFormRouteStringException("The required request field \"ufprt\" is not present.");
                }

                if (!EncryptionHelper.DecryptAndValidateEncryptedRouteString(_dataProtectionProvider, ufprt, out var additionalDataParts))
                {
                    throw new HttpUmbracoFormRouteStringException("The Umbraco form request route string could not be decrypted.");
                }

                if (!additionalDataParts[ViewConstants.ReservedAdditionalKeys.Controller].InvariantEquals(currentController) ||
                    !additionalDataParts[ViewConstants.ReservedAdditionalKeys.Action].InvariantEquals(currentAction) ||
                    (!additionalDataParts[ViewConstants.ReservedAdditionalKeys.Area].IsNullOrWhiteSpace() && !additionalDataParts[ViewConstants.ReservedAdditionalKeys.Area].InvariantEquals(currentArea)))
                {
                    throw new HttpUmbracoFormRouteStringException("The provided Umbraco form request route string was meant for a different controller and action.");
                }

            }
        }
    }
}
