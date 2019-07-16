using System;
using System.Net;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Represents an attribute that is used to prevent an invalid Umbraco form request route string on a request.
    /// </summary>
    /// <seealso cref="System.Web.Mvc.FilterAttribute" />
    /// <seealso cref="System.Web.Mvc.IAuthorizationFilter" />
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
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var ufprt = filterContext.HttpContext.Request["ufprt"];
            if (ufprt.IsNullOrWhiteSpace())
            {
                throw new HttpUmbracoFormRouteStringException("The required Umbraco request data is invalid.");
            }

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(ufprt, out var additionalDataParts))
            {
                throw new HttpUmbracoFormRouteStringException("The required Umbraco request data is invalid.");
            }

            if (additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Controller] != filterContext.ActionDescriptor.ControllerDescriptor.ControllerName ||
                additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Action] != filterContext.ActionDescriptor.ActionName ||
                additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Area].NullOrWhiteSpaceAsNull() != filterContext.RouteData.DataTokens["area"]?.ToString().NullOrWhiteSpaceAsNull())
            {
                throw new HttpUmbracoFormRouteStringException("The required Umbraco request data is invalid.");
            }
        }
    }
}
