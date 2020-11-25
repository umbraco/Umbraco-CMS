using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Attribute used to check that the request contains a valid Umbraco form request string.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.FilterAttribute" />
    /// <seealso cref="System.Web.Mvc.IAuthorizationFilter" />
    /// <remarks>
    /// Applying this attribute/filter to a <see cref="SurfaceController"/> or SurfaceController Action will ensure that the Action can only be executed
    /// when it is routed to from within Umbraco, typically when rendering a form with BeginUmbracoForm. It will mean that the natural MVC route for this Action
    /// will fail with a <see cref="HttpUmbracoFormRouteStringException"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateUmbracoFormRouteStringAttribute : FilterAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        /// <exception cref="ArgumentNullException">filterContext</exception>
        /// <exception cref="Umbraco.Web.Mvc.HttpUmbracoFormRouteStringException">The required request field \"ufprt\" is not present.
        /// or
        /// The Umbraco form request route string could not be decrypted.
        /// or
        /// The provided Umbraco form request route string was meant for a different controller and action.</exception>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            var ufprt = filterContext.HttpContext.Request["ufprt"];
            ValidateRouteString(ufprt, filterContext.ActionDescriptor?.ControllerDescriptor.ControllerName, filterContext.ActionDescriptor?.ActionName, filterContext.RouteData?.DataTokens["area"]?.ToString());
        }
        public void ValidateRouteString(string ufprt, string currentController, string currentAction, string currentArea)
        {
            if (ufprt.IsNullOrWhiteSpace())
            {
                throw new HttpUmbracoFormRouteStringException("The required request field \"ufprt\" is not present.");
            }

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(ufprt, out var additionalDataParts))
            {
                throw new HttpUmbracoFormRouteStringException("The Umbraco form request route string could not be decrypted.");
            }

            if (!additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Controller].InvariantEquals(currentController) ||
                !additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Action].InvariantEquals(currentAction) ||
                (!additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Area].IsNullOrWhiteSpace() && !additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Area].InvariantEquals(currentArea)))
            {
                throw new HttpUmbracoFormRouteStringException("The provided Umbraco form request route string was meant for a different controller and action.");
            }

        }
    }
}
