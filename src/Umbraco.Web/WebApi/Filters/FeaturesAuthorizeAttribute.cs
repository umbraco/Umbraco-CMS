using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Web.Features;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Will return unauthorized for the controller if it's been globally disabled
    /// </summary>
    public sealed class FeaturesAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var controllerType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            return FeaturesResolver.Current.Features.DisabledFeatures.Controllers.Contains(controllerType) == false;
        }
    }
}