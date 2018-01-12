using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Web.Features;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the controller is an authorized feature.
    /// </summary>
    /// <remarks>Else returns unauthorized.</remarks>
    public sealed class FeatureAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var controllerType = actionContext.ControllerContext.ControllerDescriptor.ControllerType;
            return FeaturesResolver.Current.Features.IsEnabled(controllerType);
        }
    }
}